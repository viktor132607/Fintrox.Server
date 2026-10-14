using System.Globalization;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>
/// Restricted period-end foreign bank revaluation. Does not infer quantities
/// from unmatched transactions or silently treat native and base as comparable.
/// </summary>
public sealed partial class BankReconciliationService
{
    public async Task<ForeignBankRevaluationResponse?> RevalueForeignBankAsync(
        Guid statementId, PostForeignBankRevaluationRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = organization.RequireOrganizationId();
        if (statementId == Guid.Empty ||
            request.ExchangeRateId == Guid.Empty ||
            request.FxGainAccountId == Guid.Empty ||
            request.FxLossAccountId == Guid.Empty ||
            request.FxGainAccountId == request.FxLossAccountId ||
            decimal.Round(request.ExpectedNativeClosingBalance, 4) !=
                request.ExpectedNativeClosingBalance ||
            decimal.Round(request.ExpectedLedgerBaseBalance, 4) !=
                request.ExpectedLedgerBaseBalance)
            throw new BankReconciliationConflictException(
                "Revaluation requires a valid cutoff statement, rate, distinct gain/loss accounts and exact four-decimal expected balances.");

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
            var cn = (NpgsqlConnection)db.Database.GetDbConnection();

            Guid accountId;
            Guid glId;
            string quoteCurrency;
            string baseCurrency;
            DateOnly cutoff;
            decimal statementClosing;
            await using (var statement = Command(cn, tx, """
                SELECT a.id,a.gl_account_id,a.currency_code,
                    o.base_currency_code,s.to_date,s.closing_balance
                FROM banking.bank_statements s
                JOIN banking.bank_accounts a
                  ON a.id=s.bank_account_id AND a.organization_id=s.organization_id
                JOIN core.organizations o ON o.id=s.organization_id
                WHERE s.organization_id=@org AND s.id=@statement
                """, ("org", tenant), ("statement", statementId)))
            await using (var reader = await statement.ExecuteReaderAsync(cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken)) return null;
                accountId = reader.GetGuid(0);
                glId = reader.GetGuid(1);
                quoteCurrency = reader.GetString(2);
                baseCurrency = reader.GetString(3);
                cutoff = reader.GetFieldValue<DateOnly>(4);
                statementClosing = reader.GetDecimal(5);
            }

            if (quoteCurrency == baseCurrency)
                throw new BankReconciliationConflictException(
                    "Base-currency accounts have no foreign bank revaluation.");
            // A negative native bank balance represents an overdraft with
            // liability FX signs, not the Asset FX policy implemented here.
            if (statementClosing < 0m)
                throw new BankReconciliationConflictException(
                    "Foreign bank overdrafts require a separate liability valuation policy.");
            if (statementClosing != request.ExpectedNativeClosingBalance)
                throw new BankReconciliationConflictException(
                    "Expected native closing balance changed; refresh statement evidence.");

            // Import, settlement and revaluation always lock account first.
            await using (var bankLock = Command(cn, tx, """
                SELECT id FROM banking.bank_accounts
                WHERE organization_id=@org AND id=@account FOR UPDATE
                """, ("org", tenant), ("account", accountId)))
                await bankLock.ExecuteScalarAsync(cancellationToken);

            // Immutable revaluation replay is checked before period status.
            // This preserves idempotency after a legitimate later close.
            await using (var existing = Command(cn, tx, """
                SELECT rv.exchange_rate_id,rv.gain_account_id,rv.loss_account_id,
                    rv.native_closing_balance,rv.ledger_base_before,
                    rv.approved_rate,rv.target_base_balance,rv.adjustment_base,
                    rv.journal_entry_id,e.status
                FROM banking.bank_fx_revaluations rv
                LEFT JOIN accounting.journal_entries e
                  ON e.id=rv.journal_entry_id AND e.organization_id=rv.organization_id
                WHERE rv.organization_id=@org AND rv.statement_id=@statement
                """, ("org", tenant), ("statement", statementId)))
            await using (var reader = await existing.ExecuteReaderAsync(cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    var recordedJournal = reader.IsDBNull(8) ? (Guid?)null :
                        reader.GetGuid(8);
                    if (reader.GetGuid(0) != request.ExchangeRateId ||
                        reader.GetGuid(1) != request.FxGainAccountId ||
                        reader.GetGuid(2) != request.FxLossAccountId ||
                        reader.GetDecimal(3) != request.ExpectedNativeClosingBalance ||
                        reader.GetDecimal(4) != request.ExpectedLedgerBaseBalance ||
                        (recordedJournal.HasValue &&
                         (reader.IsDBNull(9) || reader.GetString(9) != "Posted")))
                        throw new BankReconciliationConflictException(
                            "Bank cutoff already revalued with different approved terms or the historical journal is no longer posted.");

                    var delta = reader.GetDecimal(7);
                    return new ForeignBankRevaluationResponse(
                        statementId, accountId, recordedJournal, cutoff,
                        quoteCurrency, reader.GetDecimal(3),
                        reader.GetDecimal(5), reader.GetDecimal(4),
                        reader.GetDecimal(6), delta,
                        delta > 0m ? delta : 0m,
                        delta < 0m ? -delta : 0m,
                        AlreadyRevalued: true);
                }
            }

            await using (var olderRevaluation = Command(cn, tx, """
                SELECT EXISTS(SELECT 1 FROM banking.bank_fx_revaluations
                    WHERE organization_id=@org AND bank_account_id=@account
                      AND cutoff_date>=@cutoff)
                """, ("org", tenant), ("account", accountId), ("cutoff", cutoff)))
            {
                if ((bool)(await olderRevaluation.ExecuteScalarAsync(cancellationToken))!)
                    throw new BankReconciliationConflictException(
                        "Bank revaluation must progress forward by cutoff date and cannot replace a previously recorded period.");
            }

            // The statement must exactly finish an existing fiscal period.
            // Currency quantity from other overlapping/gapped records cannot
            // be considered complete settlement evidence.
            var initialPeriod = await calendars.FindPeriodByDateAsync(
                tenant, cutoff, trackChanges: false, cancellationToken);
            if (initialPeriod is null || initialPeriod.EndDate != cutoff)
                throw new BankReconciliationConflictException(
                    "Bank revaluation requires an exact accounting period-end statement.");

            // Fiscal-year/period row locks serialize against hard-close;
            // the later GL and settlement reads observe the locked calendar.
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
                cutoff != period.EndDate ||
                cutoff < year.StartDate || cutoff > year.EndDate)
                throw new BankReconciliationConflictException(
                    "Fiscal year/period is closed or does not match the bank revaluation cutoff.");

            var gainsAndLosses = new[] { request.FxGainAccountId, request.FxLossAccountId };
            if (gainsAndLosses.Contains(glId))
                throw new BankReconciliationConflictException(
                    "FX result account cannot be the bank asset account.");
            var activeResults = await db.Accounts.AsNoTracking()
                .Where(a => a.OrganizationId == tenant &&
                    gainsAndLosses.Contains(a.Id) && a.IsActive && a.IsAnalytical)
                .Select(a => new { a.Id, a.Type })
                .ToArrayAsync(cancellationToken);
            if (activeResults.Length != 2 ||
                !activeResults.Any(a => a.Id == request.FxGainAccountId &&
                    a.Type == AccountType.Revenue) ||
                !activeResults.Any(a => a.Id == request.FxLossAccountId &&
                    a.Type == AccountType.Expense))
                throw new BankReconciliationConflictException(
                    "Revaluation gain and loss require tenant-owned active analytical Revenue and Expense GL accounts.");

            var rate = await (
                from fx in db.ExchangeRates.AsNoTracking()
                join baseUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.BaseCurrencyId, fx.OrganizationId }
                    equals new { baseUnit.Id, baseUnit.OrganizationId }
                join quoteUnit in db.Currencies.AsNoTracking()
                    on new { Id = fx.QuoteCurrencyId, fx.OrganizationId }
                    equals new { quoteUnit.Id, quoteUnit.OrganizationId }
                where fx.Id == request.ExchangeRateId &&
                      fx.OrganizationId == tenant && fx.EffectiveDate == cutoff &&
                      baseUnit.Code == baseCurrency && quoteUnit.Code == quoteCurrency &&
                      baseUnit.IsActive && quoteUnit.IsActive
                select new { fx.Rate }
            ).SingleOrDefaultAsync(cancellationToken);
            if (rate is null || rate.Rate <= 0m)
                throw new BankReconciliationConflictException(
                    "Approved FX rate for the exact period-end cutoff and correct currency pair is missing.");

            // Verify the entire native quantity chain, not just current page:
            // first statement starts at 0, following statements exactly
            // follow previous closing and contiguous calendar dates.
            var chain = new List<(Guid Id, DateOnly Start, DateOnly End,
                decimal Opening, decimal Closing)>();
            await using (var statementRows = Command(cn, tx, """
                SELECT id,from_date,to_date,opening_balance,closing_balance
                FROM banking.bank_statements
                WHERE organization_id=@org AND bank_account_id=@account
                  AND from_date<=@cutoff
                ORDER BY from_date,id
                """, ("org", tenant), ("account", accountId), ("cutoff", cutoff)))
            await using (var reader = await statementRows.ExecuteReaderAsync(cancellationToken))
                while (await reader.ReadAsync(cancellationToken))
                    chain.Add((reader.GetGuid(0),
                        reader.GetFieldValue<DateOnly>(1),
                        reader.GetFieldValue<DateOnly>(2),
                        reader.GetDecimal(3), reader.GetDecimal(4)));

            if (chain.Count == 0 || chain[^1].Id != statementId ||
                chain[0].Opening != 0m || chain[^1].End != cutoff)
                throw new BankReconciliationConflictException(
                    "Complete native bank history requires an initial zero opening and latest cutoff statement.");

            for (var i = 1; i < chain.Count; i++)
            {
                if (chain[i].Start != chain[i - 1].End.AddDays(1) ||
                    chain[i].Opening != chain[i - 1].Closing)
                    throw new BankReconciliationConflictException(
                        "Foreign bank statements must form a continuous balance-consistent history.");
            }

            // Do not accept any bank asset posting before the first imported
            // statement. An unknown starting base carrying value cannot be
            // revalued using a zero native opening assumption.
            await using (var earlyGl = Command(cn, tx, """
                SELECT count(*) FROM accounting.journal_lines j
                JOIN accounting.journal_entries e
                  ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@ledger
                  AND e.posting_date<@start
                  AND e.status IN ('Posted','Reversed')
                """, ("org", tenant), ("ledger", glId),
                ("start", chain[0].Start)))
            {
                if (Convert.ToInt64(await earlyGl.ExecuteScalarAsync(cancellationToken)) != 0)
                    throw new BankReconciliationConflictException(
                        "Bank GL had activity before the first zero-opening statement; migrate and reconcile starting balances first.");
            }

            // Validate that every native movement within the statement chain
            // has a unique Posted System settlement on this bank GL and that
            // each statement's signed movement balances its closing amount.
            foreach (var item in chain)
            {
                decimal amount;
                await using (var native = Command(cn, tx, """
                    SELECT coalesce(sum(signed_amount),0)
                    FROM banking.bank_statement_lines
                    WHERE organization_id=@org AND statement_id=@statement
                    """, ("org", tenant), ("statement", item.Id)))
                    amount = (decimal)(await native.ExecuteScalarAsync(cancellationToken))!;

                if (item.Opening + amount != item.Closing)
                    throw new BankReconciliationConflictException(
                        "Bank statement native line sum does not equal its signed closing movement.");
            }

            await using (var missing = Command(cn, tx, """
                SELECT count(*)
                FROM banking.bank_statement_lines l
                JOIN banking.bank_statements s
                  ON s.id=l.statement_id AND s.organization_id=l.organization_id
                LEFT JOIN banking.bank_fx_settlements fx
                  ON fx.statement_line_id=l.id AND fx.organization_id=l.organization_id
                LEFT JOIN accounting.journal_entries e
                  ON e.id=fx.journal_entry_id AND e.organization_id=fx.organization_id
                WHERE l.organization_id=@org AND s.bank_account_id=@account
                  AND s.to_date<=@cutoff
                  AND (l.matched_payment_id IS NOT NULL
                    OR fx.id IS NULL OR e.status IS DISTINCT FROM 'Posted'
                    OR e.source IS DISTINCT FROM 'System'
                    OR e.posting_date<>l.value_date
                    OR e.external_reference IS DISTINCT FROM
                       'bank-fx-settlement:v1:' || replace(l.id::text,'-','')
                    OR NOT EXISTS (
                        SELECT 1 FROM accounting.journal_lines j
                        WHERE j.organization_id=@org AND j.journal_entry_id=e.id
                          AND j.account_id=@ledger
                          AND j.debit-j.credit=CASE WHEN l.signed_amount>0
                            THEN fx.bank_base_amount ELSE -fx.bank_base_amount END)
                    OR EXISTS (
                        SELECT 1 FROM banking.bank_fee_postings fee
                        WHERE fee.organization_id=@org AND fee.statement_line_id=l.id))
                """, ("org", tenant), ("account", accountId),
                ("cutoff", cutoff), ("ledger", glId)))
            {
                if (Convert.ToInt64(await missing.ExecuteScalarAsync(cancellationToken)) != 0)
                    throw new BankReconciliationConflictException(
                        "Every native bank line up to cutoff must be posted once with valid foreign settlement GL evidence before revaluation.");
            }

            // Other GL movements (manual journals, base-currency postings,
            // undocumented opening balances) make the native/base projection
            // uncertifiable. Previous FX revaluations are explicitly allowed.
            await using (var unrecognized = Command(cn, tx, """
                SELECT count(*) FROM accounting.journal_lines j
                JOIN accounting.journal_entries e
                  ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@ledger
                  AND e.posting_date<=@cutoff
                  AND e.status IN ('Posted','Reversed')
                  AND (e.status<>'Posted' OR
                      NOT EXISTS (
                          SELECT 1 FROM banking.bank_fx_settlements fx
                          JOIN banking.bank_statement_lines l
                            ON l.id=fx.statement_line_id
                           AND l.organization_id=fx.organization_id
                          JOIN banking.bank_statements s
                            ON s.id=l.statement_id
                           AND s.organization_id=l.organization_id
                          WHERE fx.organization_id=@org
                            AND fx.journal_entry_id=e.id
                            AND s.bank_account_id=@account
                      ) AND NOT EXISTS (
                          SELECT 1 FROM banking.bank_fx_revaluations rv
                          WHERE rv.organization_id=@org
                            AND rv.journal_entry_id=e.id
                            AND rv.bank_account_id=@account
                      ))
                """, ("org", tenant), ("ledger", glId),
                ("cutoff", cutoff), ("account", accountId)))
            {
                if (Convert.ToInt64(await unrecognized.ExecuteScalarAsync(cancellationToken)) != 0)
                    throw new BankReconciliationConflictException(
                        "Bank GL contains unrecognized or reversed entries. Resolve the source-to-ledger reconciliation before revaluation.");
            }

            decimal glBefore;
            await using (var balance = Command(cn, tx, """
                SELECT coalesce(sum(j.debit-j.credit),0)
                FROM accounting.journal_lines j
                JOIN accounting.journal_entries e
                  ON e.id=j.journal_entry_id AND e.organization_id=j.organization_id
                WHERE j.organization_id=@org AND j.account_id=@ledger
                  AND e.posting_date<=@cutoff
                  AND e.status IN ('Posted','Reversed')
                """, ("org", tenant), ("ledger", glId), ("cutoff", cutoff)))
                glBefore = (decimal)(await balance.ExecuteScalarAsync(cancellationToken))!;

            if (glBefore != request.ExpectedLedgerBaseBalance)
                throw new BankReconciliationConflictException(
                    "Cumulative posted bank GL base balance changed; refresh and reapprove the valuation.");

            var target = decimal.Round(
                statementClosing / rate.Rate, 4, MidpointRounding.AwayFromZero);
            var delta = target - glBefore;
            var now = clock.GetUtcNow();
            Guid? journalId = null;
            if (delta != 0m)
            {
                var reference = $"bank-fx-revaluation:v1:{accountId:N}:{cutoff:yyyyMMdd}";
                if (await journals.GetSystemEntryByExternalReferenceAsync(
                    tenant, reference, trackChanges: false,
                    cancellationToken) is not null)
                    throw new BankReconciliationConflictException(
                        "Unlinked FX revaluation journal exists for this bank period.");

                var entry = JournalEntry.CreateDraft(
                    tenant, period.Id, cutoff, cutoff,
                    "Approved period-end unrealized bank currency revaluation",
                    JournalEntrySource.System, reference, now);
                await journals.AddEntryAsync(entry, cancellationToken);
                var adjustment = Math.Abs(delta);
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, entry.Id, 1,
                    delta > 0m ? glId : request.FxLossAccountId,
                    adjustment, 0m,
                    delta > 0m ? "Unrealized bank FX uplift" :
                        "Unrealized bank FX expense", now),
                    cancellationToken);
                await journals.AddLineAsync(JournalLine.Create(
                    tenant, entry.Id, 2,
                    delta > 0m ? request.FxGainAccountId : glId,
                    0m, adjustment,
                    delta > 0m ? "Unrealized FX income" :
                        "Unrealized bank FX write-down", now),
                    cancellationToken);
                await journals.SaveChangesAsync(cancellationToken);
                var sequence = await journals.AllocatePostingSequenceAsync(
                    tenant, year.Id, cancellationToken);
                entry.Post(string.Create(CultureInfo.InvariantCulture,
                    $"{year.StartDate:yyyyMMdd}-{sequence:D6}"), now);
                await journals.SaveChangesAsync(cancellationToken);
                journalId = entry.Id;
            }

            await using (var save = Command(cn, tx, """
                INSERT INTO banking.bank_fx_revaluations
                    (id,organization_id,bank_account_id,statement_id,cutoff_date,
                     exchange_rate_id,approved_rate,native_closing_balance,
                     ledger_base_before,target_base_balance,adjustment_base,
                     gain_account_id,loss_account_id,journal_entry_id)
                VALUES (@id,@org,@account,@statement,@cutoff,
                    @rateId,@rate,@native,@before,@target,@delta,
                    @gain,@loss,@journal)
                """, ("id", Guid.NewGuid()), ("org", tenant),
                ("account", accountId), ("statement", statementId),
                ("cutoff", cutoff), ("rateId", request.ExchangeRateId),
                ("rate", rate.Rate), ("native", statementClosing),
                ("before", glBefore), ("target", target),
                ("delta", delta), ("gain", request.FxGainAccountId),
                ("loss", request.FxLossAccountId),
                ("journal", (object?)journalId ?? DBNull.Value)))
                await save.ExecuteNonQueryAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);
            return new ForeignBankRevaluationResponse(
                statementId, accountId, journalId, cutoff,
                quoteCurrency, statementClosing, rate.Rate,
                glBefore, target, delta,
                delta > 0m ? delta : 0m, delta < 0m ? -delta : 0m,
                AlreadyRevalued: false);
        });
    }
}
