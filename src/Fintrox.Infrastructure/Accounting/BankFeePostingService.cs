using System.Globalization;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// One bank statement fee expense posting, tied to an immutable imported
/// negative bank line. This never modifies the imported bank evidence.
/// </summary>
public sealed partial class BankReconciliationService
{
    public async Task<BankFeePostingResponse?> PostFeeAsync(
        Guid statementId,
        Guid lineId,
        PostBankFeeRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var reason = request.Reason?.Trim();
        if (statementId == Guid.Empty || lineId == Guid.Empty ||
            request.ExpenseAccountId == Guid.Empty ||
            string.IsNullOrWhiteSpace(reason) ||
            reason.Length is < 10 or > 250 ||
            request.ExpectedSignedAmount >= 0m ||
            decimal.Round(request.ExpectedSignedAmount, 4) != request.ExpectedSignedAmount)
        {
            throw new BankReconciliationConflictException(
                "Fee requires negative four-decimal signed amount, a valid expense account and a 10-250 character reason.");
        }

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();

            Guid bankAccountId;
            Guid bankGlId;
            await using (var lookup = Command(cn, tx, """
                SELECT a.id, a.gl_account_id FROM banking.bank_statements s
                JOIN banking.bank_accounts a
                  ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                WHERE s.id=@statement AND s.organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            await using (var reader = await lookup.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                bankAccountId = reader.GetGuid(0);
                bankGlId = reader.GetGuid(1);
            }

            // Consistent lock order with statement import and manual match.
            await using (var accountLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE id=@bank AND organization_id=@org FOR UPDATE
                """, ("bank", bankAccountId), ("org", tenant)))
                await accountLock.ExecuteScalarAsync(cancellationToken);

            decimal signedAmount;
            DateOnly valueDate;
            Guid? matchedPaymentId;
            await using (var lineLock = Command(cn, tx, """
                SELECT signed_amount,value_date,matched_payment_id
                FROM banking.bank_statement_lines
                WHERE id=@line AND statement_id=@statement
                  AND organization_id=@org FOR UPDATE
                """, ("org", tenant), ("statement", statementId),
                ("line", lineId)))
            await using (var reader = await lineLock.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                signedAmount = reader.GetDecimal(0);
                valueDate = reader.GetFieldValue<DateOnly>(1);
                matchedPaymentId = reader.IsDBNull(2) ? null : reader.GetGuid(2);
            }

            if (signedAmount != request.ExpectedSignedAmount || signedAmount >= 0m)
                throw new BankReconciliationConflictException(
                    "Expected fee amount does not match the negative imported bank line.");
            if (matchedPaymentId is not null)
                throw new BankReconciliationConflictException(
                    "A bank line already matched to a payment cannot also be posted as a fee.");

            // Idempotent replay is checked BEFORE the period-open gate:
            // a legitimate retry still works after authorized period close.
            await using (var existing = Command(cn, tx, """
                SELECT f.journal_entry_id, f.expense_account_id,
                       f.reason, e.status
                FROM banking.bank_fee_postings f
                JOIN accounting.journal_entries e
                  ON e.id=f.journal_entry_id AND e.organization_id=f.organization_id
                WHERE f.organization_id=@org AND f.statement_line_id=@line
                """, ("org", tenant), ("line", lineId)))
            await using (var reader = await existing.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.GetGuid(1) != request.ExpenseAccountId ||
                        reader.GetString(2) != reason ||
                        reader.GetString(3) != "Posted")
                    {
                        throw new BankReconciliationConflictException(
                            "This bank line was already posted as a fee with different details or its journal has changed.");
                    }

                    var existingJournalId = reader.GetGuid(0);
                    return new BankFeePostingResponse(
                        statementId, lineId, existingJournalId,
                        request.ExpenseAccountId, -signedAmount, reason,
                        AlreadyPosted: true);
                }
            }

            if (request.ExpenseAccountId == bankGlId)
                throw new BankReconciliationConflictException(
                    "Bank and fee expense must be different GL accounts.");

            var expense = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(
                account => account.OrganizationId == tenant &&
                    account.Id == request.ExpenseAccountId,
                cancellationToken);
            if (expense is null || !expense.IsActive || !expense.IsAnalytical ||
                expense.Type != AccountType.Expense)
                throw new BankReconciliationConflictException(
                    "Bank fee requires an active analytical Expense account belonging to the tenant.");

            var periodBeforeLock = await calendars.FindPeriodByDateAsync(
                tenant, valueDate, trackChanges: false, cancellationToken);
            if (periodBeforeLock is null)
                throw new BankReconciliationConflictException(
                    "No fiscal accounting period contains the fee value date.");

            await journals.LockPostingCalendarAsync(
                tenant, periodBeforeLock.FiscalYearId, periodBeforeLock.Id,
                cancellationToken);
            var period = await calendars.GetPeriodAsync(
                tenant, periodBeforeLock.FiscalYearId,
                periodBeforeLock.Id, trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, periodBeforeLock.FiscalYearId,
                trackChanges: false, cancellationToken);
            if (period is null || year is null ||
                period.Status != AccountingPeriodStatus.Open ||
                year.Status != FiscalYearStatus.Open ||
                valueDate < period.StartDate || valueDate > period.EndDate ||
                valueDate < year.StartDate || valueDate > year.EndDate)
            {
                throw new BankReconciliationConflictException(
                    "Bank fee cannot post into a closed or invalid accounting period/year.");
            }

            var reference = $"bank-fee:v1:{lineId:N}";
            if (await journals.GetSystemEntryByExternalReferenceAsync(
                    tenant, reference, trackChanges: false, cancellationToken) is not null)
            {
                throw new BankReconciliationConflictException(
                    "An existing fee journal has no matching immutable bank-fee evidence.");
            }

            var now = clock.GetUtcNow();
            var amount = -signedAmount;
            var entry = JournalEntry.CreateDraft(
                tenant, period.Id, valueDate, valueDate,
                $"Bank fee: {reason}", JournalEntrySource.System,
                reference, now);
            await journals.AddEntryAsync(entry, cancellationToken);
            await journals.AddLineAsync(JournalLine.Create(
                tenant, entry.Id, 1, request.ExpenseAccountId,
                amount, 0m, "Bank fee expense", now), cancellationToken);
            await journals.AddLineAsync(JournalLine.Create(
                tenant, entry.Id, 2, bankGlId,
                0m, amount, "Bank fee paid from bank account", now),
                cancellationToken);

            // Persist Draft + lines before posting, honoring immutable
            // posted-ledger triggers in the authoritative GL schema.
            await journals.SaveChangesAsync(cancellationToken);
            var sequence = await journals.AllocatePostingSequenceAsync(
                tenant, year.Id, cancellationToken);
            entry.Post(string.Create(CultureInfo.InvariantCulture,
                $"{year.StartDate:yyyyMMdd}-{sequence:D6}"), now);
            await journals.SaveChangesAsync(cancellationToken);

            await using (var link = Command(cn, tx, """
                INSERT INTO banking.bank_fee_postings
                  (id,organization_id,statement_line_id,
                   journal_entry_id,expense_account_id,reason)
                VALUES (@id,@org,@line,@journal,@expense,@reason)
                """, ("id", Guid.NewGuid()), ("org", tenant), ("line", lineId),
                ("journal", entry.Id), ("expense", request.ExpenseAccountId),
                ("reason", reason!)))
                await link.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new BankFeePostingResponse(
                statementId, lineId, entry.Id, request.ExpenseAccountId,
                amount, reason!, AlreadyPosted: false);
        });
    }
}
