using Fintrox.Application.Accounting;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Reports;

namespace Fintrox.Application.Reports;

public sealed class AccountingReportService(
    IAccountingReportRepository repository,
    IAccountRepository accountRepository,
    ICurrentOrganization currentOrganization) : IAccountingReportService
{
    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        bool includeZeroBalances,
        CancellationToken cancellationToken)
    {
        var (from, to) = ValidateDateRange(fromDate, toDate);
        var organizationId = currentOrganization.RequireOrganizationId();

        var movements = await repository.GetTrialBalanceMovementsAsync(
            organizationId,
            from,
            to,
            cancellationToken);

        var movementByAccount = movements.ToDictionary(
            movement => movement.AccountId);

        var accounts = await accountRepository.ListAsync(
            organizationId,
            includeInactive: true,
            cancellationToken);

        var rows = new List<TrialBalanceRowResponse>(accounts.Count);

        foreach (var account in accounts.OrderBy(
                     account => account.Code,
                     StringComparer.Ordinal))
        {
            movementByAccount.TryGetValue(
                account.Id,
                out var movement);

            var openingNet = movement is null
                ? 0m
                : movement.OpeningDebit - movement.OpeningCredit;

            var periodDebit = movement?.PeriodDebit ?? 0m;
            var periodCredit = movement?.PeriodCredit ?? 0m;
            var closingNet = openingNet + periodDebit - periodCredit;

            var (openingDebit, openingCredit) = SplitNet(openingNet);
            var (closingDebit, closingCredit) = SplitNet(closingNet);

            if (!includeZeroBalances &&
                openingDebit == 0m &&
                openingCredit == 0m &&
                periodDebit == 0m &&
                periodCredit == 0m &&
                closingDebit == 0m &&
                closingCredit == 0m)
            {
                continue;
            }

            rows.Add(new TrialBalanceRowResponse(
                account.Id,
                account.Code,
                account.Name,
                account.Type.ToString(),
                account.IsActive,
                openingDebit,
                openingCredit,
                periodDebit,
                periodCredit,
                closingDebit,
                closingCredit));
        }

        return new TrialBalanceResponse(
            from,
            to,
            rows.Sum(row => row.OpeningDebit),
            rows.Sum(row => row.OpeningCredit),
            rows.Sum(row => row.PeriodDebit),
            rows.Sum(row => row.PeriodCredit),
            rows.Sum(row => row.ClosingDebit),
            rows.Sum(row => row.ClosingCredit),
            rows);
    }

    public async Task<GeneralLedgerResponse?> GetGeneralLedgerAsync(
        Guid accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var (from, to) = ValidateDateRange(fromDate, toDate);
        var organizationId = currentOrganization.RequireOrganizationId();

        var account = await accountRepository.GetAsync(
            organizationId,
            accountId,
            trackChanges: false,
            cancellationToken);

        if (account is null)
        {
            return null;
        }

        var openingNet = await repository.GetOpeningNetBalanceAsync(
            organizationId,
            accountId,
            from,
            cancellationToken);

        var movements = await repository.GetGeneralLedgerMovementsAsync(
            organizationId,
            accountId,
            from,
            to,
            cancellationToken);

        var runningNet = openingNet;
        var periodDebit = 0m;
        var periodCredit = 0m;
        var lines = new List<GeneralLedgerLineResponse>(movements.Count);

        foreach (var movement in movements)
        {
            periodDebit += movement.Debit;
            periodCredit += movement.Credit;
            runningNet += movement.Debit - movement.Credit;

            var (runningDebit, runningCredit) = SplitNet(runningNet);

            lines.Add(new GeneralLedgerLineResponse(
                movement.JournalEntryId,
                movement.JournalNumber,
                movement.PostingDate,
                movement.DocumentDate,
                movement.JournalDescription,
                movement.LineNumber,
                movement.LineDescription,
                movement.Source.ToString(),
                movement.ExternalReference,
                movement.Debit,
                movement.Credit,
                runningDebit,
                runningCredit));
        }

        var closingNet = openingNet + periodDebit - periodCredit;
        var (openingDebit, openingCredit) = SplitNet(openingNet);
        var (closingDebit, closingCredit) = SplitNet(closingNet);

        return new GeneralLedgerResponse(
            account.Id,
            account.Code,
            account.Name,
            account.Type.ToString(),
            from,
            to,
            openingDebit,
            openingCredit,
            periodDebit,
            periodCredit,
            closingDebit,
            closingCredit,
            lines);
    }

    private static (DateOnly FromDate, DateOnly ToDate) ValidateDateRange(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        if (!fromDate.HasValue || !toDate.HasValue)
        {
            throw new AccountingReportQueryException(
                "Both fromDate and toDate are required.");
        }

        if (fromDate.Value > toDate.Value)
        {
            throw new AccountingReportQueryException(
                "fromDate cannot be after toDate.");
        }

        return (fromDate.Value, toDate.Value);
    }

    private static (decimal Debit, decimal Credit) SplitNet(decimal net)
    {
        return net >= 0m
            ? (net, 0m)
            : (0m, decimal.Negate(net));
    }
}
