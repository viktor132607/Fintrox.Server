using System.Globalization;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Controlled restatement of a bank Asset cutoff FX valuation. Both the
/// old GL valuation and internal dual-review stay in immutable history.
/// </summary>
public sealed partial class BankReconciliationService
{
    public async Task<ForeignBankRevaluationAmendmentResponse?> AmendForeignBankRevaluationAsync(
        Guid statementId, AmendForeignBankRevaluationRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        var actor = currentUser.RequireUserId();
        var reason = request.Reason?.Trim();
        if (statementId == Guid.Empty || request.ExpectedCurrentVersion < 1 ||
            request.ExchangeRateId == Guid.Empty ||
            request.FxGainAccountId == Guid.Empty ||
            request.FxLossAccountId == Guid.Empty ||
            request.FxGainAccountId == request.FxLossAccountId ||
            string.IsNullOrWhiteSpace(reason) || reason.Length is < 10 or > 400 ||
            decimal.Round(request.ExpectedNativeClosingBalance, 4) != request.ExpectedNativeClosingBalance ||
            decimal.Round(request.ExpectedLedgerBaseBalance, 4) != request.ExpectedLedgerBaseBalance)
            throw new BankReconciliationConflictException(
                "Versioned revaluation correction requires a specific previous version/journal, selected cutoff FX rate, expected native/base balances and a 10-400 character reason.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();
            Guid bankId, bankGlId, originalRevaluationId;
            DateOnly cutoff;
            decimal native, ledgerBase;
            string currency, baseCurrency;
            await using (var source = Command(cn, tx, """
                SELECT s.bank_account_id,a.gl_account_id,s.to_date,
                       s.closing_balance,a.currency_code,o.base_currency_code,
                       rv.id,rv.ledger_base_before
                FROM banking.bank_statements s
                JOIN banking.bank_accounts a
                    ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                JOIN core.organizations o ON o.id=s.organization_id
                JOIN banking.bank_fx_revaluations rv
                    ON rv.statement_id=s.id AND rv.organization_id=s.organization_id
                WHERE s.id=@statement AND s.organization_id=@org
                """, ("statement", statementId), ("org", tenant)))
            await using (var reader = await source.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                bankId = reader.GetGuid(0); bankGlId = reader.GetGuid(1);
                cutoff = reader.GetFieldValue<DateOnly>(2);
                native = reader.GetDecimal(3); currency = reader.GetString(4);
                baseCurrency = reader.GetString(5);
                originalRevaluationId = reader.GetGuid(6);
                ledgerBase = reader.GetDecimal(7);
            }
            if (currency == baseCurrency || native < 0m ||
                request.ExpectedNativeClosingBalance != native)
                throw new BankReconciliationConflictException(
                    "Original cutoff bank Asset amount changed or account is not an eligible foreign-currency asset.");

            await using (var bankLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE organization_id=@org AND id=@bank FOR UPDATE
                """, ("org", tenant), ("bank", bankId)))
                await bankLock.ExecuteScalarAsync(cancellationToken);

            // No subsequent statement or revaluation may depend on a
            // historical cutoff being restated in this bounded operation.
            await using (var later = Command(cn, tx, """
                SELECT EXISTS(
                    SELECT 1 FROM banking.bank_statements s
                    WHERE s.organization_id=@org AND s.bank_account_id=@bank
                      AND s.to_date>@cutoff)
                  OR EXISTS(
                    SELECT 1 FROM banking.bank_fx_revaluations r
                    WHERE r.organization_id=@org AND r.bank_account_id=@bank
                      AND r.cutoff_date>@cutoff)
                """, ("org", tenant), ("bank", bankId), ("cutoff", cutoff)))
                if ((bool)(await later.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "A later statement/valuation depends on this cutoff. Coordinated multi-period restatement is required.");

            // Other bank GL journals dated after this cutoff depend on the
            // original historical carrying value. This endpoint does not
            // recompute later financial periods, so refuse the amendment.
            await using (var laterLedger = Command(cn, tx, """
                SELECT count(*) FROM accounting.journal_lines j
                JOIN accounting.journal_entries e
                  ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@ledger
                  AND e.posting_date>@cutoff
                  AND e.status IN ('Posted','Reversed')
                """, ("org", tenant), ("ledger", bankGlId), ("cutoff", cutoff)))
                if (Convert.ToInt64(
                    await laterLedger.ExecuteScalarAsync(cancellationToken)) != 0)
                    throw new BankReconciliationConflictException(
                        "Later bank GL movements depend on this cutoff. Use coordinated multi-period correction.");

            // Always lock year then period EXCLUSIVELY; journal posting
            // normally takes calendar FOR SHARE. No other GL mutation can
            // race this valuation replacement through the normal API.
            var initial = await calendars.FindPeriodByDateAsync(
                tenant, cutoff, trackChanges: false, cancellationToken);
            if (initial is null || initial.EndDate != cutoff)
                throw new BankReconciliationConflictException(
                    "Cutoff must equal original fiscal period end.");
            await using (var yearLock = Command(cn, tx, """
                SELECT id FROM accounting.fiscal_years
                WHERE organization_id=@org AND id=@year FOR UPDATE
                """, ("org", tenant), ("year", initial.FiscalYearId)))
                if (await yearLock.ExecuteScalarAsync(cancellationToken) is not Guid)
                    throw new BankReconciliationConflictException("Fiscal year missing.");
            await using (var periodLock = Command(cn, tx, """
                SELECT id FROM accounting.accounting_periods
                WHERE organization_id=@org AND id=@period AND fiscal_year_id=@year
                FOR UPDATE
                """, ("org", tenant), ("year", initial.FiscalYearId),
                ("period", initial.Id)))
                if (await periodLock.ExecuteScalarAsync(cancellationToken) is not Guid)
                    throw new BankReconciliationConflictException("Fiscal period missing.");

            var period = await calendars.GetPeriodAsync(
                tenant, initial.FiscalYearId, initial.Id,
                trackChanges: false, cancellationToken);
            var year = await calendars.GetFiscalYearAsync(
                tenant, initial.FiscalYearId,
                trackChanges: false, cancellationToken);
            if (period is null || year is null ||
                period.Status != AccountingPeriodStatus.Open ||
                year.Status != FiscalYearStatus.Open)
                throw new BankReconciliationConflictException(
                    "A closed fiscal period cannot be restated without an approved fiscal reopening workflow.");

            Guid? previousJournal = null;
            decimal previousTarget, previousAdjustment;
            int version;
            await using (var current = Command(cn, tx, """
                SELECT version_number,replacement_journal_id,
                       replacement_target_base,replacement_adjustment_base
                FROM banking.bank_fx_revaluation_amendments
                WHERE organization_id=@org AND statement_id=@statement
                ORDER BY version_number DESC LIMIT 1
                """, ("org", tenant), ("statement", statementId)))
            await using (var reader = await current.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    version = reader.GetInt32(0);
                    previousJournal = reader.IsDBNull(1) ? null : reader.GetGuid(1);
                    previousTarget = reader.GetDecimal(2);
                    previousAdjustment = reader.GetDecimal(3);
                }
                else
                {
                    version = 0;
                    previousTarget = 0m;
                    previousAdjustment = 0m;
                }
            }
            if (version == 0)
            {
                await using var first = Command(cn, tx, """
                    SELECT journal_entry_id,target_base_balance,adjustment_base
                    FROM banking.bank_fx_revaluations
                    WHERE id=@original AND organization_id=@org
                    """, ("original", originalRevaluationId), ("org", tenant));
                await using var reader = await first.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    throw new BankReconciliationConflictException("Original revaluation evidence missing.");
                previousJournal = reader.IsDBNull(0) ? null : reader.GetGuid(0);
                previousTarget = reader.GetDecimal(1);
                previousAdjustment = reader.GetDecimal(2);
                version = 1;
            }

            // Idempotent replay of a completed next-version write, including
            // after the first request changed the version number.
            if (version == request.ExpectedCurrentVersion + 1)
            {
                await using var prior = Command(cn, tx, """
                    SELECT previous_journal_id,reversal_journal_id,replacement_journal_id,
                           exchange_rate_id,gain_account_id,loss_account_id,
                           native_closing_balance,ledger_base_before,
                           reason,amended_by_user_id,approved_rate,
                           previous_target_base,replacement_target_base,
                           replacement_adjustment_base
                    FROM banking.bank_fx_revaluation_amendments
                    WHERE organization_id=@org AND statement_id=@statement
                      AND version_number=@version
                    """, ("org", tenant), ("statement", statementId),
                    ("version", version));
                await using var reader = await prior.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken) ||
                    (reader.IsDBNull(0) ? (Guid?)null : reader.GetGuid(0)) != request.ExpectedCurrentJournalEntryId ||
                    reader.GetGuid(3) != request.ExchangeRateId ||
                    reader.GetGuid(4) != request.FxGainAccountId ||
                    reader.GetGuid(5) != request.FxLossAccountId ||
                    reader.GetDecimal(6) != native ||
                    reader.GetDecimal(7) != request.ExpectedLedgerBaseBalance ||
                    reader.GetString(8) != reason || reader.GetGuid(9) != actor)
                    throw new BankReconciliationConflictException(
                        "The requested prior version was already corrected with different terms.");
                var revId = reader.IsDBNull(1) ? (Guid?)null : reader.GetGuid(1);
                var newId = reader.IsDBNull(2) ? (Guid?)null : reader.GetGuid(2);
                return new ForeignBankRevaluationAmendmentResponse(
                    statementId, bankId, version, request.ExpectedCurrentJournalEntryId,
                    revId, newId, ledgerBase, reader.GetDecimal(11),
                    reader.GetDecimal(12), reader.GetDecimal(13), reader.GetDecimal(10),
                    actor, AlreadyAmended: true);
            }

            if (version != request.ExpectedCurrentVersion ||
                previousJournal != request.ExpectedCurrentJournalEntryId ||
                ledgerBase != request.ExpectedLedgerBaseBalance)
                throw new BankReconciliationConflictException(
                    "Current valuation version, original bank GL carrying value or journal does not match expected state.");

            var accountIds = new[]{request.FxGainAccountId, request.FxLossAccountId};
            if (accountIds.Contains(bankGlId))
                throw new BankReconciliationConflictException(
                    "FX gain/loss account cannot equal the bank Asset GL account.");
            var active = await db.Accounts.AsNoTracking()
                .Where(a => a.OrganizationId == tenant &&
                    accountIds.Contains(a.Id) && a.IsActive && a.IsAnalytical)
                .Select(a => new { a.Id, a.Type }).ToArrayAsync(cancellationToken);
            if (active.Length != 2 ||
                !active.Any(a => a.Id == request.FxGainAccountId && a.Type == AccountType.Revenue) ||
                !active.Any(a => a.Id == request.FxLossAccountId && a.Type == AccountType.Expense))
                throw new BankReconciliationConflictException(
                    "Valid active tenant Revenue and Expense FX accounts must be selected.");

            var fxRate = await (
                from fx in db.ExchangeRates.AsNoTracking()
                join baseUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.BaseCurrencyId, fx.OrganizationId }
                    equals new { baseUnit.Id, baseUnit.OrganizationId }
                join quoteUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.QuoteCurrencyId, fx.OrganizationId }
                    equals new { quoteUnit.Id, quoteUnit.OrganizationId }
                where fx.Id == request.ExchangeRateId &&
                      fx.OrganizationId == tenant && fx.EffectiveDate == cutoff &&
                      baseUnit.Code == baseCurrency && quoteUnit.Code == currency &&
                      baseUnit.IsActive && quoteUnit.IsActive
                select new { fx.Rate }
            ).SingleOrDefaultAsync(cancellationToken);
            if (fxRate is null || fxRate.Rate <= 0m)
                throw new BankReconciliationConflictException(
                    "Selected new FX rate must match the tenant, currency pair and exact cutoff date.");

            // Refuse unexpected GL movements instead of treating a genuine
            // unauthorized accounting change as a new FX gain/loss.
            decimal currentBankLedger;
            await using (var balance = Command(cn, tx, """
                SELECT COALESCE(SUM(j.debit-j.credit),0)
                FROM accounting.journal_lines j
                JOIN accounting.journal_entries e ON
                    e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@gl
                  AND e.posting_date<=@cutoff
                  AND e.status IN ('Posted','Reversed')
                """, ("org", tenant), ("gl", bankGlId), ("cutoff", cutoff)))
                currentBankLedger = (decimal)(await balance.ExecuteScalarAsync(cancellationToken))!;
            if (currentBankLedger != previousTarget)
                throw new BankReconciliationConflictException(
                    "The bank GL has unaccounted changes since the previous version. Reconcile GL before a new cutoff valuation.");

            // Exact complete native source/FX settlement trail still needed;
            // the later read-only bridge will enforce it before approval.
            long unmatched;
            await using (var validation = Command(cn, tx, """
                SELECT count(*) FROM banking.bank_statement_lines l
                JOIN banking.bank_statements s
                  ON s.id=l.statement_id AND s.organization_id=l.organization_id
                LEFT JOIN banking.bank_fx_settlements fx
                  ON fx.statement_line_id=l.id AND fx.organization_id=l.organization_id
                LEFT JOIN accounting.journal_entries e
                  ON e.id=fx.journal_entry_id AND e.organization_id=fx.organization_id
                WHERE l.organization_id=@org AND s.bank_account_id=@bank
                  AND s.to_date<=@cutoff
                  AND (fx.id IS NULL OR e.status IS DISTINCT FROM 'Posted'
                       OR l.matched_payment_id IS NOT NULL)
                """, ("org", tenant), ("bank", bankId), ("cutoff", cutoff)))
                unmatched = Convert.ToInt64(await validation.ExecuteScalarAsync(cancellationToken));
            if (unmatched != 0)
                throw new BankReconciliationConflictException(
                    "All original foreign bank statement movements must remain posted and unmatched to ordinary payments.");

            // Re-read complete foreign statement/native source and all
            // GL valuation versions under the exclusive period lock.
            // Do not restate a partially reconciled or damaged cutoff.
            var currentEvidence = await ReadForeignCloseAsync(
                cn, tx, tenant, statementId, cancellationToken);
            if (currentEvidence is null || !currentEvidence.Report.InternallyReconciled ||
                currentEvidence.Report.VersionNumber != version)
                throw new BankReconciliationConflictException(
                    "The previous native/base bank close evidence is not fully reconciled; historical correction is unsafe.");

            var newTarget = decimal.Round(
                native / fxRate.Rate, 4, MidpointRounding.AwayFromZero);
            var newAdjustment = newTarget - ledgerBase;
            var now = clock.GetUtcNow();
            Guid? reversedId = null;
            if (previousJournal is Guid oldJournalId)
            {
                await journals.LockJournalForPostingAsync(tenant, oldJournalId, cancellationToken);
                var original = await journals.GetEntryAsync(
                    tenant, oldJournalId, trackChanges: true, cancellationToken);
                if (original is null || original.Status != JournalEntryStatus.Posted ||
                    original.Source != JournalEntrySource.System ||
                    original.PostingDate != cutoff || original.FiscalPeriodId != period.Id ||
                    original.ReversedByJournalEntryId.HasValue)
                    throw new BankReconciliationConflictException(
                        "Previous FX valuation journal is not an intact Posted System entry.");
                var expectedRef = version == 1
                    ? $"bank-fx-revaluation:v1:{bankId:N}:{cutoff:yyyyMMdd}"
                    : $"bank-fx-revaluation:v{version}:{bankId:N}:{cutoff:yyyyMMdd}";
                if (original.ExternalReference != expectedRef)
                    throw new BankReconciliationConflictException(
                        "Previous FX valuation journal reference does not match its immutable version.");
                var sourceLines = await journals.ListLinesAsync(
                    tenant, oldJournalId, cancellationToken);
                if (sourceLines.Count != 2 ||
                    sourceLines.Sum(x => x.Debit) != sourceLines.Sum(x => x.Credit))
                    throw new BankReconciliationConflictException(
                        "Previous booked revaluation journal is not balanced.");
                var reverse = JournalEntry.CreateReversalDraft(
                    tenant, period.Id, oldJournalId, cutoff,
                    $"Controlled bank FX valuation v{version} correction: {reason}", now);
                await journals.AddEntryAsync(reverse, cancellationToken);
                foreach (var line in sourceLines)
                    await journals.AddLineAsync(JournalLine.Create(
                        tenant, reverse.Id, line.LineNumber, line.AccountId,
                        line.Credit, line.Debit, line.Description, now),
                        cancellationToken);
                await journals.SaveChangesAsync(cancellationToken);
                var number = await journals.AllocatePostingSequenceAsync(
                    tenant, year.Id, cancellationToken);
                reverse.Post(string.Create(CultureInfo.InvariantCulture,
                    $"{year.StartDate:yyyyMMdd}-{number:D6}"), now);
                original.MarkReversed(reverse.Id, now);
                await journals.SaveChangesAsync(cancellationToken);
                reversedId = reverse.Id;
            }
            else if (previousAdjustment != 0m)
                throw new BankReconciliationConflictException(
                    "Previous nonzero valuation has no original posted journal.");

            Guid? replacementId = null;
            if (newAdjustment != 0m)
            {
                var reference = $"bank-fx-revaluation:v{version+1}:{bankId:N}:{cutoff:yyyyMMdd}";
                if (await journals.GetSystemEntryByExternalReferenceAsync(
                    tenant, reference, trackChanges: false, cancellationToken) is not null)
                    throw new BankReconciliationConflictException(
                        "Next version GL reference already exists without the expected amendment.");
                var replacement = JournalEntry.CreateDraft(
                    tenant, period.Id, cutoff, cutoff,
                    $"Bank currency valuation version {version+1}: {reason}",
                    JournalEntrySource.System, reference, now);
                await journals.AddEntryAsync(replacement, cancellationToken);
                var change = Math.Abs(newAdjustment);
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, replacement.Id, 1,
                    newAdjustment > 0m ? bankGlId : request.FxLossAccountId,
                    change, 0m, "Corrected foreign bank valuation", now),
                    cancellationToken);
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, replacement.Id, 2,
                    newAdjustment > 0m ? request.FxGainAccountId : bankGlId,
                    0m, change, "Corrected unrealized FX result", now),
                    cancellationToken);
                await journals.SaveChangesAsync(cancellationToken);
                var number = await journals.AllocatePostingSequenceAsync(
                    tenant, year.Id, cancellationToken);
                replacement.Post(string.Create(CultureInfo.InvariantCulture,
                    $"{year.StartDate:yyyyMMdd}-{number:D6}"), now);
                await journals.SaveChangesAsync(cancellationToken);
                replacementId = replacement.Id;
            }

            await using (var save = Command(cn, tx, """
                INSERT INTO banking.bank_fx_revaluation_amendments (
                    id,organization_id,original_revaluation_id,bank_account_id,
                    statement_id,cutoff_date,version_number,
                    previous_journal_id,reversal_journal_id,replacement_journal_id,
                    exchange_rate_id,approved_rate,native_closing_balance,
                    ledger_base_before,previous_target_base,previous_adjustment_base,
                    replacement_target_base,replacement_adjustment_base,
                    gain_account_id,loss_account_id,amended_by_user_id,reason)
                VALUES (@id,@org,@original,@bank,@statement,@cutoff,@version,
                    @prior,@reverse,@replacement,@rateId,@rate,@native,
                    @ledgerBase,@oldTarget,@oldAdjustment,@target,@adjustment,
                    @gain,@loss,@actor,@reason)
                """, ("id", Guid.NewGuid()), ("org", tenant),
                ("original", originalRevaluationId), ("bank", bankId),
                ("statement", statementId), ("cutoff", cutoff),
                ("version", version+1),
                ("prior", (object?)previousJournal ?? DBNull.Value),
                ("reverse", (object?)reversedId ?? DBNull.Value),
                ("replacement", (object?)replacementId ?? DBNull.Value),
                ("rateId", request.ExchangeRateId), ("rate", fxRate.Rate),
                ("native", native), ("ledgerBase", ledgerBase),
                ("oldTarget", previousTarget), ("oldAdjustment", previousAdjustment),
                ("target", newTarget), ("adjustment", newAdjustment),
                ("gain", request.FxGainAccountId),
                ("loss", request.FxLossAccountId),
                ("actor", actor), ("reason", reason!)))
                await save.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return new ForeignBankRevaluationAmendmentResponse(
                statementId, bankId, version+1, previousJournal, reversedId,
                replacementId, ledgerBase, previousTarget, newTarget,
                newAdjustment, fxRate.Rate, actor, AlreadyAmended: false);
        });
    }
}
