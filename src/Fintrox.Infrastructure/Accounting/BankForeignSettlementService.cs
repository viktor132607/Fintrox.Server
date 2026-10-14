using System.Globalization;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Posts an explicitly approved foreign bank movement against an AR/AP
/// clearing carrying balance, with any REALIZED difference to FX P&L.
/// Imported native bank evidence and GL history remain immutable.
/// </summary>
public sealed partial class BankReconciliationService
{
    public async Task<ForeignBankSettlementResponse?> PostForeignSettlementAsync(
        Guid statementId, Guid lineId,
        PostForeignBankSettlementRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (statementId == Guid.Empty || lineId == Guid.Empty ||
            request.ExchangeRateId == Guid.Empty ||
            request.ClearingAccountId == Guid.Empty ||
            request.FxGainAccountId == Guid.Empty ||
            request.FxLossAccountId == Guid.Empty ||
            request.ClearingBaseAmount <= 0m ||
            decimal.Round(request.ClearingBaseAmount, 4) != request.ClearingBaseAmount ||
            request.ExpectedSignedForeignAmount == 0m ||
            decimal.Round(request.ExpectedSignedForeignAmount, 4) !=
                request.ExpectedSignedForeignAmount)
            throw new BankReconciliationConflictException(
                "Foreign settlement needs existing rate and account IDs, a positive four-decimal clearing carrying value and exact signed native amount.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            Guid bankAccountId;
            Guid bankGlId;
            string currency;
            string baseCurrency;
            await using (var lookup = Command(cn, tx, """
                SELECT a.id,a.gl_account_id,a.currency_code,o.base_currency_code
                FROM banking.bank_statements s
                JOIN banking.bank_accounts a ON
                    a.id=s.bank_account_id AND a.organization_id=s.organization_id
                JOIN core.organizations o ON o.id=s.organization_id
                WHERE s.id=@statement AND s.organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            await using (var reader = await lookup.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                bankAccountId = reader.GetGuid(0);
                bankGlId = reader.GetGuid(1);
                currency = reader.GetString(2);
                baseCurrency = reader.GetString(3);
            }
            if (currency == baseCurrency)
                throw new BankReconciliationConflictException(
                    "Use the base-currency banking workflow for this account.");

            await using (var bankLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE id=@id AND organization_id=@org FOR UPDATE
                """, ("id", bankAccountId), ("org", tenant)))
                await bankLock.ExecuteScalarAsync(cancellationToken);

            decimal signedNative;
            DateOnly valueDate;
            Guid? matchedPaymentId;
            await using (var lineLock = Command(cn, tx, """
                SELECT signed_amount,value_date,matched_payment_id
                FROM banking.bank_statement_lines
                WHERE id=@line AND statement_id=@statement
                  AND organization_id=@org FOR UPDATE
                """, ("line", lineId), ("statement", statementId), ("org", tenant)))
            await using (var reader = await lineLock.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                signedNative = reader.GetDecimal(0);
                valueDate = reader.GetFieldValue<DateOnly>(1);
                matchedPaymentId = reader.IsDBNull(2) ? null : reader.GetGuid(2);
            }

            if (signedNative != request.ExpectedSignedForeignAmount)
                throw new BankReconciliationConflictException(
                    "Foreign bank amount differs from immutable statement evidence.");
            if (matchedPaymentId.HasValue)
                throw new BankReconciliationConflictException(
                    "Matched payment bank line cannot also be a foreign settlement.");
            await using (var priorFee = Command(cn, tx, """
                SELECT EXISTS (
                    SELECT 1 FROM banking.bank_fee_postings
                    WHERE organization_id=@org AND statement_line_id=@line)
                """, ("org", tenant), ("line", lineId)))
                if ((bool)(await priorFee.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "Previously posted bank fee cannot also be a foreign settlement.");

            // Idempotency uses immutable metadata, never today's edited FX rate.
            await using (var replay = Command(cn, tx, """
                SELECT f.journal_entry_id,f.exchange_rate_id,
                    f.clearing_account_id,f.clearing_base_amount,
                    f.gain_account_id,f.loss_account_id,
                    f.approved_rate,f.bank_base_amount,
                    f.realized_gain_base,f.realized_loss_base,e.status
                FROM banking.bank_fx_settlements f
                JOIN accounting.journal_entries e ON
                    e.id=f.journal_entry_id AND e.organization_id=f.organization_id
                WHERE f.organization_id=@org AND f.statement_line_id=@line
                """, ("org", tenant), ("line", lineId)))
            await using (var reader = await replay.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.GetGuid(1) != request.ExchangeRateId ||
                        reader.GetGuid(2) != request.ClearingAccountId ||
                        reader.GetDecimal(3) != request.ClearingBaseAmount ||
                        reader.GetGuid(4) != request.FxGainAccountId ||
                        reader.GetGuid(5) != request.FxLossAccountId ||
                        reader.GetString(10) != "Posted")
                        throw new BankReconciliationConflictException(
                            "Foreign bank line already settled with different terms or a changed journal.");

                    return new ForeignBankSettlementResponse(
                        statementId, lineId, reader.GetGuid(0),
                        currency, signedNative, reader.GetDecimal(6),
                        reader.GetDecimal(7), reader.GetDecimal(3),
                        reader.GetDecimal(8), reader.GetDecimal(9),
                        AlreadyPosted: true);
                }
            }

            // A previously certified period-end valuation must not be
            // invalidated by a later backdated settlement posting.
            await using (var lockedCutoff = Command(cn, tx, """
                SELECT EXISTS (SELECT 1 FROM banking.bank_fx_revaluations
                    WHERE organization_id=@org AND bank_account_id=@account
                      AND cutoff_date>=@date)
                """, ("org", tenant), ("account", bankAccountId),
                ("date", valueDate)))
                if ((bool)(await lockedCutoff.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "This bank value date was already covered by an approved FX revaluation. A controlled correction is required.");

            if (new[] { bankGlId, request.ClearingAccountId,
                request.FxGainAccountId, request.FxLossAccountId }
                .Distinct().Count() != 4)
                throw new BankReconciliationConflictException(
                    "Bank, clearing and FX gain/loss must be four distinct accounts.");

            var ids = new[] { request.ClearingAccountId,
                request.FxGainAccountId, request.FxLossAccountId };
            var accountTypes = await db.Accounts.AsNoTracking()
                .Where(a => a.OrganizationId == tenant && ids.Contains(a.Id) &&
                    a.IsActive && a.IsAnalytical)
                .Select(a => new { a.Id, a.Type })
                .ToArrayAsync(cancellationToken);
            if (accountTypes.Length != 3 ||
                !accountTypes.Any(a => a.Id == request.ClearingAccountId &&
                    (a.Type == AccountType.Asset || a.Type == AccountType.Liability)) ||
                !accountTypes.Any(a => a.Id == request.FxGainAccountId &&
                    a.Type == AccountType.Revenue) ||
                !accountTypes.Any(a => a.Id == request.FxLossAccountId &&
                    a.Type == AccountType.Expense))
                throw new BankReconciliationConflictException(
                    "Clearing must be analytical Asset/Liability; FX gain Revenue; FX loss Expense.");

            // Rate convention in Fintrox: quote units per base unit.
            // Require the tenant's approved original effective-date rate;
            // never silently fetch latest or derive it from a description.
            var rate = await (
                from fx in db.ExchangeRates.AsNoTracking()
                join baseUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.BaseCurrencyId, fx.OrganizationId }
                    equals new { baseUnit.Id, baseUnit.OrganizationId }
                join quoteUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.QuoteCurrencyId, fx.OrganizationId }
                    equals new { quoteUnit.Id, quoteUnit.OrganizationId }
                where fx.Id == request.ExchangeRateId &&
                      fx.OrganizationId == tenant &&
                      fx.EffectiveDate == valueDate &&
                      baseUnit.Code == baseCurrency &&
                      quoteUnit.Code == currency &&
                      baseUnit.IsActive && quoteUnit.IsActive
                select new { fx.Rate }
            ).SingleOrDefaultAsync(cancellationToken);
            if (rate is null || rate.Rate <= 0m)
                throw new BankReconciliationConflictException(
                    "An approved tenant FX rate for this exact date and base/quote currency pair is required.");

            var bankBase = decimal.Round(
                Math.Abs(signedNative) / rate.Rate, 4,
                MidpointRounding.AwayFromZero);
            if (bankBase <= 0m)
                throw new BankReconciliationConflictException(
                    "Native amount is too small to book at four base-currency decimals.");

            var delta = bankBase - request.ClearingBaseAmount;
            var incoming = signedNative > 0m;
            var gain = (incoming ? delta : -delta) > 0m
                ? Math.Abs(delta) : 0m;
            var loss = (incoming ? delta : -delta) < 0m
                ? Math.Abs(delta) : 0m;

            var initialPeriod = await calendars.FindPeriodByDateAsync(
                tenant, valueDate, trackChanges: false, cancellationToken);
            if (initialPeriod is null)
                throw new BankReconciliationConflictException(
                    "No fiscal period includes the foreign bank line date.");
            await journals.LockPostingCalendarAsync(
                tenant, initialPeriod.FiscalYearId, initialPeriod.Id,
                cancellationToken);
            var period = await calendars.GetPeriodAsync(
                tenant, initialPeriod.FiscalYearId, initialPeriod.Id,
                trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, initialPeriod.FiscalYearId,
                trackChanges: false, cancellationToken);
            if (period is null || year is null ||
                period.Status != AccountingPeriodStatus.Open ||
                year.Status != FiscalYearStatus.Open ||
                valueDate < period.StartDate || valueDate > period.EndDate ||
                valueDate < year.StartDate || valueDate > year.EndDate)
                throw new BankReconciliationConflictException(
                    "Foreign settlement cannot post into a closed/misdated financial period.");

            var reference = $"bank-fx-settlement:v1:{lineId:N}";
            if (await journals.GetSystemEntryByExternalReferenceAsync(
                tenant, reference, trackChanges: false, cancellationToken) is not null)
                throw new BankReconciliationConflictException(
                    "An existing FX bank journal lacks the matching immutable settlement link.");

            var now = clock.GetUtcNow();
            var journal = JournalEntry.CreateDraft(
                tenant, period.Id, valueDate, valueDate,
                "Approved foreign currency bank clearing and realized FX result",
                JournalEntrySource.System, reference, now);
            await journals.AddEntryAsync(journal, cancellationToken);
            var number = 1;
            async Task Add(Guid account, decimal debit, decimal credit, string description) =>
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, journal.Id, number++, account,
                    debit, credit, description, now), cancellationToken);

            await Add(bankGlId, incoming ? bankBase : 0m,
                incoming ? 0m : bankBase, "Foreign bank movement at approved rate");
            await Add(request.ClearingAccountId,
                incoming ? 0m : request.ClearingBaseAmount,
                incoming ? request.ClearingBaseAmount : 0m,
                "Settle receivable/payable carrying value");
            if (gain > 0m)
                await Add(request.FxGainAccountId, 0m, gain,
                    "Realized foreign exchange gain");
            if (loss > 0m)
                await Add(request.FxLossAccountId, loss, 0m,
                    "Realized foreign exchange loss");

            await journals.SaveChangesAsync(cancellationToken);
            var sequence = await journals.AllocatePostingSequenceAsync(
                tenant, year.Id, cancellationToken);
            journal.Post(string.Create(CultureInfo.InvariantCulture,
                $"{year.StartDate:yyyyMMdd}-{sequence:D6}"), now);
            await journals.SaveChangesAsync(cancellationToken);

            await using (var link = Command(cn, tx, """
                INSERT INTO banking.bank_fx_settlements(
                    id,organization_id,statement_line_id,journal_entry_id,
                    exchange_rate_id,approved_rate,clearing_account_id,
                    clearing_base_amount,bank_base_amount,realized_gain_base,
                    realized_loss_base,gain_account_id,loss_account_id)
                VALUES (@id,@org,@line,@journal,@rateId,@rate,
                    @clearing,@clearingAmount,@bankAmount,@gain,@loss,
                    @gainAccount,@lossAccount)
                """,
                ("id", Guid.NewGuid()), ("org", tenant),
                ("line", lineId), ("journal", journal.Id),
                ("rateId", request.ExchangeRateId), ("rate", rate.Rate),
                ("clearing", request.ClearingAccountId),
                ("clearingAmount", request.ClearingBaseAmount),
                ("bankAmount", bankBase), ("gain", gain), ("loss", loss),
                ("gainAccount", request.FxGainAccountId),
                ("lossAccount", request.FxLossAccountId)))
                await link.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new ForeignBankSettlementResponse(
                statementId, lineId, journal.Id, currency, signedNative,
                rate.Rate, bankBase, request.ClearingBaseAmount,
                gain, loss, AlreadyPosted: false);
        });
    }
}
