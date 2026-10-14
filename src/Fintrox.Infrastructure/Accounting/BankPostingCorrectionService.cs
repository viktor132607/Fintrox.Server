using System.Globalization;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Append-only, tenant-scoped compensation of posted bank fee and realized
/// foreign settlement journals. Statement evidence is never edited.
/// Revalued/approved foreign cutoffs require a different coordinated process.
/// </summary>
public sealed partial class BankReconciliationService
{
    public async Task<BankPostingCorrectionResponse?> CorrectBankPostingAsync(
        Guid statementId, Guid statementLineId,
        CorrectBankPostingRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        var sourceKind = request.SourceKind?.Trim();
        var reason = request.Reason?.Trim();
        if (statementId == Guid.Empty || statementLineId == Guid.Empty ||
            request.OriginalJournalEntryId == Guid.Empty ||
            sourceKind is not ("BankFee" or "ForeignSettlement") ||
            string.IsNullOrWhiteSpace(reason) || reason.Length is < 10 or > 400)
            throw new BankReconciliationConflictException(
                "Correction requires an original posted bank source journal, BankFee/ForeignSettlement kind and a 10–400 character reason.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();

            Guid bankAccount;
            await using (var findBank = Command(cn, tx, """
                SELECT bank_account_id FROM banking.bank_statements
                WHERE id=@statement AND organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            {
                if (await findBank.ExecuteScalarAsync(cancellationToken) is not Guid id)
                    return null;
                bankAccount = id;
            }

            // Order: tenant bank account -> source bank line -> fiscal
            // calendar year/period -> original journal. Consistent with
            // existing import, bank payment and fee/FX posting operations.
            await using (var bankLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE organization_id=@org AND id=@bank FOR UPDATE
                """, ("org", tenant), ("bank", bankAccount)))
                await bankLock.ExecuteScalarAsync(cancellationToken);

            DateOnly originalValueDate;
            await using (var lineLock = Command(cn, tx, """
                SELECT value_date FROM banking.bank_statement_lines
                WHERE id=@line AND statement_id=@statement
                  AND organization_id=@org FOR UPDATE
                """, ("line", statementLineId), ("statement", statementId),
                ("org", tenant)))
            {
                if (await lineLock.ExecuteScalarAsync(cancellationToken)
                    is not DateOnly valueDate)
                    return null;
                originalValueDate = valueDate;
            }

            // A corrected source must have an exact original immutable link.
            // Do not allow a caller to reverse arbitrary GL journals.
            var evidenceQuery = sourceKind == "BankFee"
                ? """
                  SELECT journal_entry_id FROM banking.bank_fee_postings
                  WHERE organization_id=@org AND statement_line_id=@line
                  """
                : """
                  SELECT journal_entry_id FROM banking.bank_fx_settlements
                  WHERE organization_id=@org AND statement_line_id=@line
                  """;
            await using (var evidence = Command(cn, tx, evidenceQuery,
                ("org", tenant), ("line", statementLineId)))
            {
                if (await evidence.ExecuteScalarAsync(cancellationToken)
                    is not Guid originalEvidence ||
                    originalEvidence != request.OriginalJournalEntryId)
                    throw new BankReconciliationConflictException(
                        "The supplied journal does not belong to this bank line and source kind.");
            }

            await using (var prior = Command(cn, tx, """
                SELECT original_journal_id,reversal_journal_id,
                    source_kind,correction_date,reason,corrected_by_user_id
                FROM banking.bank_posting_corrections
                WHERE organization_id=@org AND statement_line_id=@line
                  AND source_kind=@kind
                """, ("org", tenant), ("line", statementLineId),
                ("kind", sourceKind!)))
            await using (var reader = await prior.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.GetGuid(0) != request.OriginalJournalEntryId ||
                        reader.GetString(2) != sourceKind ||
                        reader.GetFieldValue<DateOnly>(3) != request.CorrectionPostingDate ||
                        reader.GetString(4) != reason ||
                        reader.GetGuid(5) != actor)
                        throw new BankReconciliationConflictException(
                            "This bank posting has already been corrected with different terms.");

                    return new BankPostingCorrectionResponse(
                        statementId, statementLineId, sourceKind!,
                        request.OriginalJournalEntryId, reader.GetGuid(1),
                        request.CorrectionPostingDate, actor,
                        AlreadyCorrected: true);
                }
            }

            if (sourceKind == "ForeignSettlement")
            {
                // An immutable bank FX revaluation, or a prepared/approved
                // close, includes this posting in its original carrying
                // value. Correcting only one side would violate the signed
                // historical evidence. Refuse instead of silently rewriting.
                await using var closed = Command(cn, tx, """
                    SELECT EXISTS (
                        SELECT 1 FROM banking.bank_fx_revaluations
                        WHERE organization_id=@org AND bank_account_id=@bank
                          AND cutoff_date>=@originalDate
                    ) OR EXISTS (
                        SELECT 1 FROM banking.bank_fx_close_preparations
                        WHERE organization_id=@org AND bank_account_id=@bank
                          AND cutoff_date>=@originalDate
                    )
                    """, ("org", tenant), ("bank", bankAccount),
                    ("originalDate", originalValueDate));
                if ((bool)(await closed.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "An FX valuation or prepared review already includes this bank movement. Use a coordinated historical correction workflow.");
            }

            if (request.CorrectionPostingDate < originalValueDate)
                throw new BankReconciliationConflictException(
                    "The correction date cannot precede the original bank movement.");

            var initial = await calendars.FindPeriodByDateAsync(
                tenant, request.CorrectionPostingDate,
                trackChanges: false, cancellationToken);
            if (initial is null)
                throw new BankReconciliationConflictException(
                    "No fiscal period contains the requested correction posting date.");
            await journals.LockPostingCalendarAsync(
                tenant, initial.FiscalYearId, initial.Id,
                cancellationToken);
            var period = await calendars.GetPeriodAsync(
                tenant, initial.FiscalYearId, initial.Id,
                trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, initial.FiscalYearId,
                trackChanges: false, cancellationToken);
            if (period is null || year is null ||
                period.Status != AccountingPeriodStatus.Open ||
                year.Status != FiscalYearStatus.Open ||
                request.CorrectionPostingDate < period.StartDate ||
                request.CorrectionPostingDate > period.EndDate ||
                request.CorrectionPostingDate < year.StartDate ||
                request.CorrectionPostingDate > year.EndDate)
                throw new BankReconciliationConflictException(
                    "The correction must post into a currently open fiscal year and period.");

            await journals.LockJournalForPostingAsync(
                tenant, request.OriginalJournalEntryId, cancellationToken);
            var original = await journals.GetEntryAsync(
                tenant, request.OriginalJournalEntryId,
                trackChanges: true, cancellationToken);
            var expectedReference = sourceKind == "BankFee"
                ? $"bank-fee:v1:{statementLineId:N}"
                : $"bank-fx-settlement:v1:{statementLineId:N}";
            if (original is null || original.Status != JournalEntryStatus.Posted ||
                original.Source != JournalEntrySource.System ||
                original.ExternalReference != expectedReference ||
                original.ReversalOfJournalEntryId.HasValue ||
                original.ReversedByJournalEntryId.HasValue ||
                original.PostingDate != originalValueDate)
                throw new BankReconciliationConflictException(
                    "The original journal is not an intact posted System bank source.");

            var lines = await journals.ListLinesAsync(
                tenant, original.Id, cancellationToken);
            if (lines.Count < 2 ||
                lines.Sum(x => x.Debit) != lines.Sum(x => x.Credit))
                throw new BankReconciliationConflictException(
                    "Source journal is not balanced or has insufficient lines.");

            var now = clock.GetUtcNow();
            var reversal = JournalEntry.CreateReversalDraft(
                tenant, period.Id, original.Id,
                request.CorrectionPostingDate,
                $"Controlled bank {sourceKind} correction: {reason}", now);
            await journals.AddEntryAsync(reversal, cancellationToken);
            foreach (var line in lines)
            {
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, reversal.Id, line.LineNumber, line.AccountId,
                    line.Credit, line.Debit, line.Description, now),
                    cancellationToken);
            }
            // Persist drafts before changing GL journal lifecycle: the DB
            // trigger forbids inserting lines to an already Posted journal.
            await journals.SaveChangesAsync(cancellationToken);
            var sequence = await journals.AllocatePostingSequenceAsync(
                tenant, year.Id, cancellationToken);
            reversal.Post(string.Create(CultureInfo.InvariantCulture,
                $"{year.StartDate:yyyyMMdd}-{sequence:D6}"), now);
            original.MarkReversed(reversal.Id, now);
            await journals.SaveChangesAsync(cancellationToken);

            await using (var save = Command(cn, tx, """
                INSERT INTO banking.bank_posting_corrections (
                    id,organization_id,statement_line_id,source_kind,
                    original_journal_id,reversal_journal_id,correction_date,
                    reason,corrected_by_user_id)
                VALUES (@id,@org,@line,@kind,@original,@reversal,
                    @date,@reason,@actor)
                """, ("id", Guid.NewGuid()), ("org", tenant),
                ("line", statementLineId), ("kind", sourceKind!),
                ("original", original.Id), ("reversal", reversal.Id),
                ("date", request.CorrectionPostingDate),
                ("reason", reason!), ("actor", actor)))
                await save.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new BankPostingCorrectionResponse(
                statementId, statementLineId, sourceKind!,
                original.Id, reversal.Id, request.CorrectionPostingDate,
                actor, AlreadyCorrected: false);
        });
    }
}
