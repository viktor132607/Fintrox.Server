using System.Globalization;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

/// <summary>
/// First-book, single-currency P&amp;L transfer to retained earnings.
/// Year and final-period closing belong to the same transaction as the journal.
/// </summary>
public sealed class ProfitLossClosingService(
    IFiscalCalendarRepository calendar,
    IJournalRepository journals,
    IAccountRepository accounts,
    ITransactionRunner transactions,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IProfitLossClosingService
{
    public async Task<ProfitLossCloseResponse?> CloseAsync(
        Guid fiscalYearId,
        Guid retainedEarningsAccountId,
        CancellationToken cancellationToken)
    {
        if (fiscalYearId == Guid.Empty || retainedEarningsAccountId == Guid.Empty)
        {
            throw new FiscalCalendarConflictException(
                "Fiscal year and retained earnings account ids are required.");
        }

        var organizationId = currentOrganization.RequireOrganizationId();
        return await transactions.ExecuteAsync(async ct =>
        {
            // Exclusive year lock serializes every cooperating source/GL posting
            // and every fiscal calendar operation, across all API instances.
            await calendar.LockFiscalYearForCloseAsync(
                organizationId, fiscalYearId, ct);

            var year = await calendar.GetFiscalYearAsync(
                organizationId, fiscalYearId, trackChanges: true, ct);
            if (year is null) return null;

            var closingEntries = await journals.ListYearEndClosingEntriesAsync(
                organizationId, fiscalYearId, ct);
            var (latest, latestRound) = FindLatestClosing(closingEntries, fiscalYearId);

            if (year.Status == FiscalYearStatus.Closed)
            {
                if (latest is null || latest.Status != JournalEntryStatus.Posted)
                {
                    throw new FiscalCalendarConflictException(
                        "The year is closed without a valid active profit/loss closing journal.");
                }

                var previousLines = await journals.ListLinesAsync(
                    organizationId, latest.Id, ct);
                var previousAccounts = await accounts.ListByIdsAsync(
                    organizationId, previousLines.Select(line => line.AccountId)
                        .Distinct().ToArray(), ct);
                var equityLines = previousLines.Where(line =>
                    previousAccounts.TryGetValue(line.AccountId, out var account) &&
                    account.Type == AccountType.Equity).ToArray();

                if (equityLines.Length > 0 &&
                    equityLines.Any(line => line.AccountId != retainedEarningsAccountId))
                {
                    throw new FiscalCalendarConflictException(
                        "The fiscal year was already closed to another equity account.");
                }

                if (equityLines.Length > 1)
                {
                    throw new FiscalCalendarConflictException(
                        "The previous closing journal has invalid equity allocations.");
                }

                return new ProfitLossCloseResponse(
                    fiscalYearId, latest.Id,
                    equityLines.Length == 0
                        ? 0m
                        : equityLines[0].Credit - equityLines[0].Debit,
                    AlreadyClosed: true);
            }

            if (year.Status != FiscalYearStatus.Open)
            {
                throw new FiscalCalendarConflictException(
                    "Profit/loss closing requires an open fiscal year.");
            }

            if (latest is not null &&
                (latest.Status != JournalEntryStatus.Reversed ||
                 latest.ReversedByJournalEntryId is null))
            {
                throw new FiscalCalendarConflictException(
                    "An earlier closing journal is still active. Use the controlled correction workflow before closing again.");
            }

            var newRound = latestRound + 1;
            var reference = newRound == 1
                ? $"profit-loss-close:v1:{fiscalYearId:N}"
                : $"profit-loss-close:v1:{fiscalYearId:N}:r{newRound}";

            var equity = await accounts.GetAsync(
                organizationId, retainedEarningsAccountId,
                trackChanges: false, ct);
            if (equity is null || !equity.IsActive ||
                !equity.IsAnalytical ||
                equity.Type != AccountType.Equity)
            {
                throw new FiscalCalendarConflictException(
                    "Select an active, analytical, organization-owned equity account for retained earnings.");
            }

            var periods = (await calendar.ListPeriodsAsync(
                organizationId, fiscalYearId, ct))
                .OrderBy(period => period.StartDate).ToArray();

            if (periods.Length == 0)
            {
                throw new FiscalCalendarConflictException(
                    "Fiscal closing requires configured accounting periods.");
            }

            // Validate a complete, non-overlapping calendar before certifying
            // the year. Other than the last period, all must be fully Closed.
            var expectedDate = year.StartDate;
            for (var index = 0; index < periods.Length; index++)
            {
                var period = periods[index];
                if (period.StartDate != expectedDate ||
                    period.EndDate < period.StartDate ||
                    period.EndDate > year.EndDate)
                {
                    throw new FiscalCalendarConflictException(
                        "The fiscal calendar has gaps, overlaps or dates outside the fiscal year.");
                }

                // Legacy/backfilled journals might predate period-close
                // guards. Refuse certification if any entry's posting date
                // is outside the fiscal period it references.
                if (await calendar.HasMisdatedJournalsInPeriodAsync(
                        organizationId, period.Id,
                        period.StartDate, period.EndDate, ct))
                {
                    throw new FiscalCalendarConflictException(
                        "A journal entry has a posting date outside its assigned fiscal period.");
                }

                var last = index == periods.Length - 1;
                if (last)
                {
                    if (period.EndDate != year.EndDate ||
                        period.Status != AccountingPeriodStatus.Open)
                    {
                        throw new FiscalCalendarConflictException(
                            "The final accounting period must cover the fiscal year end and remain open for the closing entry.");
                    }
                }
                else if (period.Status != AccountingPeriodStatus.Closed)
                {
                    throw new FiscalCalendarConflictException(
                        "All preceding accounting periods must be closed before year-end profit/loss transfer.");
                }

                if (!last)
                {
                    expectedDate = period.EndDate.AddDays(1);
                }
            }

            if (await calendar.HasDraftJournalsInFiscalYearAsync(
                    organizationId, fiscalYearId, ct))
            {
                throw new FiscalCalendarConflictException(
                    "Resolve every draft journal in this fiscal year before closing.");
            }

            var finalPeriod = periods[^1];
            await calendar.LockAccountingPeriodForCloseAsync(
                organizationId, fiscalYearId, finalPeriod.Id, ct);
            var trackedFinalPeriod = await calendar.GetPeriodAsync(
                organizationId, fiscalYearId, finalPeriod.Id,
                trackChanges: true, ct);
            if (trackedFinalPeriod is null ||
                trackedFinalPeriod.Status != AccountingPeriodStatus.Open)
            {
                throw new FiscalCalendarConflictException(
                    "The final accounting period changed before closing.");
            }

            var balances = (await journals.GetProfitLossBalancesAsync(
                organizationId, year.StartDate, year.EndDate, ct))
                .Where(balance => balance.Debit != balance.Credit)
                .OrderBy(balance => balance.AccountId)
                .ToArray();

            if (balances.Length == 0)
            {
                throw new FiscalCalendarConflictException(
                    "There are no outstanding profit/loss account balances to close. Use normal fiscal close if the year has no P&L activity.");
            }

            var sourceAccounts = await accounts.ListByIdsAsync(
                organizationId, balances.Select(b => b.AccountId).ToArray(), ct);
            if (sourceAccounts.Count != balances.Length ||
                sourceAccounts.Values.Any(account =>
                    !account.IsActive ||
                    account.Type is not (AccountType.Revenue or AccountType.Expense)))
            {
                throw new FiscalCalendarConflictException(
                    "All income/expense ledger accounts must exist and be active before closing.");
            }

            var netIncome = balances.Sum(balance => balance.Credit - balance.Debit);
            var now = timeProvider.GetUtcNow();
            var entry = JournalEntry.CreateDraft(
                organizationId, trackedFinalPeriod.Id, year.EndDate, year.EndDate,
                $"Fiscal year {year.StartDate:yyyy-MM-dd} profit/loss closing",
                JournalEntrySource.System, reference, now);

            await journals.AddEntryAsync(entry, ct);
            var lineNumber = 1;
            foreach (var balance in balances)
            {
                var netDebit = balance.Debit - balance.Credit;
                await journals.AddLineAsync(
                    JournalLine.Create(
                        organizationId, entry.Id, lineNumber++,
                        balance.AccountId,
                        netDebit < 0m ? -netDebit : 0m,
                        netDebit > 0m ? netDebit : 0m,
                        "Close income/expense to retained earnings", now),
                    ct);
            }

            if (netIncome != 0m)
            {
                await journals.AddLineAsync(
                    JournalLine.Create(
                        organizationId, entry.Id, lineNumber,
                        retainedEarningsAccountId,
                        netIncome < 0m ? -netIncome : 0m,
                        netIncome > 0m ? netIncome : 0m,
                        "Annual net profit/loss transferred to equity", now),
                    ct);
            }

            // First persist the draft and lines. Existing DB immutability
            // triggers prohibit adding lines to an already-posted journal.
            await journals.SaveChangesAsync(ct);

            var sequence = await journals.AllocatePostingSequenceAsync(
                organizationId, fiscalYearId, ct);
            var number = string.Create(
                CultureInfo.InvariantCulture,
                $"{year.StartDate:yyyyMMdd}-{sequence:D6}");
            entry.Post(number, now);

            // Posting and both close states commit as one unit.
            trackedFinalPeriod.Close(now);
            year.Close(now);
            await journals.SaveChangesAsync(ct);

            return new ProfitLossCloseResponse(
                fiscalYearId, entry.Id, netIncome, AlreadyClosed: false);
        }, cancellationToken);
    }
    private static (JournalEntry? Latest, int Round) FindLatestClosing(
        IReadOnlyList<JournalEntry> entries,
        Guid fiscalYearId)
    {
        var prefix = $"profit-loss-close:v1:{fiscalYearId:N}";
        var rounds = new List<(JournalEntry Entry, int Round)>();
        foreach (var entry in entries)
        {
            if (entry.ExternalReference is null)
            {
                throw new FiscalCalendarConflictException(
                    "A year-end journal has no external reference.");
            }

            var value = entry.ExternalReference;
            int round;
            if (value == prefix)
            {
                round = 1;
            }
            else if (value.StartsWith(prefix + ":r", StringComparison.Ordinal) &&
                int.TryParse(
                    value.AsSpan(prefix.Length + 2),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var parsed) && parsed >= 2)
            {
                round = parsed;
            }
            else
            {
                throw new FiscalCalendarConflictException(
                    "An unsupported year-end closing journal reference was found.");
            }

            rounds.Add((entry, round));
        }

        var latest = rounds.OrderBy(item => item.Round).LastOrDefault();
        return latest.Entry is null
            ? (null, 0)
            : (latest.Entry, latest.Round);
    }


}
