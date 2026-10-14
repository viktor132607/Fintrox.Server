using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public sealed class FiscalCalendarService(
    IFiscalCalendarRepository repository,
    IJournalRepository journals,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IFiscalCalendarService
{
    public async Task<IReadOnlyList<FiscalYearResponse>> ListFiscalYearsAsync(
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var fiscalYears = await repository.ListFiscalYearsAsync(
            organizationId,
            cancellationToken);

        return fiscalYears
            .OrderByDescending(year => year.StartDate)
            .Select(MapFiscalYear)
            .ToArray();
    }

    public async Task<FiscalYearResponse?> GetFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var fiscalYear = await repository.GetFiscalYearAsync(
            organizationId,
            fiscalYearId,
            trackChanges: false,
            cancellationToken);

        return fiscalYear is null ? null : MapFiscalYear(fiscalYear);
    }

    public async Task<FiscalYearResponse> CreateFiscalYearAsync(
        CreateFiscalYearRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRange(request.StartDate, request.EndDate, "Fiscal year");

        var organizationId = currentOrganization.RequireOrganizationId();
        var normalizedName = request.Name.Trim();

        await EnsureFiscalYearUniqueAsync(
            organizationId,
            normalizedName,
            request.StartDate,
            request.EndDate,
            excludingFiscalYearId: null,
            cancellationToken);

        var fiscalYear = FiscalYear.Create(
            organizationId,
            normalizedName,
            request.StartDate,
            request.EndDate,
            timeProvider.GetUtcNow());

        await repository.AddFiscalYearAsync(fiscalYear, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return MapFiscalYear(fiscalYear);
    }

    public async Task<FiscalYearResponse?> UpdateFiscalYearAsync(
        Guid fiscalYearId,
        UpdateFiscalYearRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRange(request.StartDate, request.EndDate, "Fiscal year");
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                // Prevent concurrent close and period creation while editing
                // the year bounds or name.
                await repository.LockFiscalYearForCloseAsync(
                    organizationId, fiscalYearId, ct);

                var year = await repository.GetFiscalYearAsync(
                    organizationId, fiscalYearId, trackChanges: true, ct);
                if (year is null) return null;
                EnsureFiscalYearOpen(year);

                var name = request.Name.Trim();
                await EnsureFiscalYearUniqueAsync(
                    organizationId, name, request.StartDate, request.EndDate,
                    fiscalYearId, ct);

                if (await repository.HasPeriodOutsideRangeAsync(
                        organizationId, fiscalYearId,
                        request.StartDate, request.EndDate, ct))
                {
                    throw new FiscalCalendarConflictException(
                        "The fiscal year cannot exclude existing accounting periods.");
                }

                year.Update(name, request.StartDate, request.EndDate,
                    timeProvider.GetUtcNow());
                await repository.SaveChangesAsync(ct);
                return MapFiscalYear(year);
            },
            cancellationToken);
    }

    public Task<FiscalYearResponse?> SoftCloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken) =>
        ChangeFiscalYearStatusAsync(
            fiscalYearId,
            FiscalYearStatus.SoftClosed,
            cancellationToken);

    public Task<FiscalYearResponse?> CloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken) =>
        ChangeFiscalYearStatusAsync(
            fiscalYearId,
            FiscalYearStatus.Closed,
            cancellationToken);

    public Task<FiscalYearResponse?> ReopenFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken) =>
        ChangeFiscalYearStatusAsync(
            fiscalYearId,
            FiscalYearStatus.Open,
            cancellationToken);

    private async Task<FiscalYearResponse?> ChangeFiscalYearStatusAsync(
        Guid fiscalYearId,
        FiscalYearStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                // Exclusive year lock waits for in-flight GL posting and
                // serializes concurrent close/reopen requests across workers.
                await repository.LockFiscalYearForCloseAsync(
                    organizationId, fiscalYearId, ct);

                var fiscalYear = await repository.GetFiscalYearAsync(
                    organizationId, fiscalYearId, trackChanges: true, ct);

                if (fiscalYear is null)
                {
                    return null;
                }

                if (targetStatus == FiscalYearStatus.Open)
                {
                    fiscalYear.Reopen(timeProvider.GetUtcNow());
                }
                else
                {
                    if (fiscalYear.Status == FiscalYearStatus.Closed &&
                        targetStatus == FiscalYearStatus.SoftClosed)
                    {
                        throw new FiscalCalendarConflictException(
                            "A closed fiscal year must be reopened before it can be soft-closed.");
                    }

                    await EnsureFiscalYearHasPeriodsAsync(
                        organizationId, fiscalYearId, ct);

                    if (await repository.HasDraftJournalsInFiscalYearAsync(
                            organizationId, fiscalYearId, ct))
                    {
                        throw new FiscalCalendarConflictException(
                            "All draft journal entries must be posted or resolved before fiscal-year closing.");
                    }

                    if (targetStatus == FiscalYearStatus.SoftClosed)
                    {
                        if (await repository.HasOpenPeriodsAsync(
                                organizationId, fiscalYearId, ct))
                        {
                            throw new FiscalCalendarConflictException(
                                "All accounting periods must be soft-closed or closed before the fiscal year can be soft-closed.");
                        }

                        fiscalYear.SoftClose(timeProvider.GetUtcNow());
                    }
                    else if (targetStatus == FiscalYearStatus.Closed)
                    {
                        if (!await repository.AllPeriodsClosedAsync(
                                organizationId, fiscalYearId, ct))
                        {
                            throw new FiscalCalendarConflictException(
                                "All accounting periods must be closed before the fiscal year can be closed.");
                        }

                        // A status-only year close must not bypass the
                        // profit/loss transfer: all income/expense balances
                        // must already be cleared in this fiscal year.
                        var outstanding = await journals.GetProfitLossBalancesAsync(
                            organizationId, fiscalYear.StartDate, fiscalYear.EndDate, ct);
                        if (outstanding.Any(balance => balance.Debit != balance.Credit))
                        {
                            throw new FiscalCalendarConflictException(
                                "Income and expense accounts still have open balances. Run the year-end profit/loss transfer before closing the fiscal year.");
                        }

                        fiscalYear.Close(timeProvider.GetUtcNow());
                    }
                }

                await repository.SaveChangesAsync(ct);
                return MapFiscalYear(fiscalYear);
            },
            cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingPeriodResponse>> ListPeriodsAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        if (await repository.GetFiscalYearAsync(
                organizationId,
                fiscalYearId,
                trackChanges: false,
                cancellationToken) is null)
        {
            return [];
        }

        var periods = await repository.ListPeriodsAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        return periods
            .OrderBy(period => period.Number)
            .ThenBy(period => period.StartDate)
            .Select(MapPeriod)
            .ToArray();
    }

    public async Task<AccountingPeriodResponse?> GetPeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var period = await repository.GetPeriodAsync(
            organizationId,
            fiscalYearId,
            periodId,
            trackChanges: false,
            cancellationToken);

        return period is null ? null : MapPeriod(period);
    }

    public async Task<AccountingPeriodResponse> CreatePeriodAsync(
        Guid fiscalYearId,
        CreateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRange(request.StartDate, request.EndDate, "Accounting period");
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                // All calendar edits and close transitions lock year first.
                // The final Open check cannot become stale before insert.
                await repository.LockFiscalYearForCloseAsync(
                    organizationId, fiscalYearId, ct);

                var year = await RequireOpenFiscalYearAsync(
                    organizationId, fiscalYearId, ct);
                EnsurePeriodInsideFiscalYear(
                    year, request.StartDate, request.EndDate);

                await EnsurePeriodUniqueAsync(
                    organizationId, fiscalYearId,
                    request.Number, request.StartDate, request.EndDate,
                    excludingPeriodId: null, ct);

                var period = AccountingPeriod.Create(
                    organizationId, fiscalYearId,
                    request.Number, request.Name,
                    request.StartDate, request.EndDate,
                    timeProvider.GetUtcNow());
                await repository.AddPeriodAsync(period, ct);
                await repository.SaveChangesAsync(ct);
                return MapPeriod(period);
            },
            cancellationToken);
    }

    public async Task<AccountingPeriodResponse?> UpdatePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        UpdateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRange(request.StartDate, request.EndDate, "Accounting period");
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                await repository.LockFiscalYearForCloseAsync(
                    organizationId, fiscalYearId, ct);
                var year = await RequireOpenFiscalYearAsync(
                    organizationId, fiscalYearId, ct);

                await repository.LockAccountingPeriodForCloseAsync(
                    organizationId, fiscalYearId, periodId, ct);
                var period = await repository.GetPeriodAsync(
                    organizationId, fiscalYearId, periodId,
                    trackChanges: true, ct);
                if (period is null) return null;

                if (period.Status != AccountingPeriodStatus.Open)
                {
                    throw new FiscalCalendarConflictException(
                        "Only an open accounting period can be modified.");
                }

                EnsurePeriodInsideFiscalYear(
                    year, request.StartDate, request.EndDate);

                // A preexisting journal must never be stranded outside
                // the amended range of its declared fiscal period.
                if (await repository.HasMisdatedJournalsInPeriodAsync(
                        organizationId, periodId,
                        request.StartDate, request.EndDate, ct))
                {
                    throw new FiscalCalendarConflictException(
                        "Cannot change period dates to exclude an existing journal posting date.");
                }

                await EnsurePeriodUniqueAsync(
                    organizationId, fiscalYearId,
                    request.Number, request.StartDate, request.EndDate,
                    periodId, ct);

                period.Update(
                    request.Number, request.Name,
                    request.StartDate, request.EndDate,
                    timeProvider.GetUtcNow());
                await repository.SaveChangesAsync(ct);
                return MapPeriod(period);
            },
            cancellationToken);
    }

    public Task<AccountingPeriodResponse?> SoftClosePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            fiscalYearId,
            periodId,
            AccountingPeriodStatus.SoftClosed,
            cancellationToken);
    }

    public Task<AccountingPeriodResponse?> ClosePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            fiscalYearId,
            periodId,
            AccountingPeriodStatus.Closed,
            cancellationToken);
    }

    public Task<AccountingPeriodResponse?> ReopenPeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        return ChangePeriodStatusAsync(
            fiscalYearId,
            periodId,
            AccountingPeriodStatus.Open,
            cancellationToken);
    }

    private async Task<AccountingPeriodResponse?> ChangePeriodStatusAsync(
        Guid fiscalYearId,
        Guid periodId,
        AccountingPeriodStatus targetStatus,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();

        return await transactionRunner.ExecuteAsync(
            async ct =>
            {
                // Lock YEAR first, then PERIOD: posting takes the same order.
                // Recheck both states after any concurrent writer commits.
                await repository.LockFiscalYearForCloseAsync(
                    organizationId, fiscalYearId, ct);

                var fiscalYear = await repository.GetFiscalYearAsync(
                    organizationId, fiscalYearId, trackChanges: false, ct);

                if (fiscalYear is null)
                {
                    throw new FiscalCalendarConflictException(
                        "The fiscal year does not exist in this organization.");
                }

                EnsureFiscalYearOpen(fiscalYear);

                await repository.LockAccountingPeriodForCloseAsync(
                    organizationId, fiscalYearId, periodId, ct);

                var accountingPeriod = await repository.GetPeriodAsync(
                    organizationId, fiscalYearId, periodId, trackChanges: true, ct);

                if (accountingPeriod is null)
                {
                    return null;
                }

                var now = timeProvider.GetUtcNow();
                switch (targetStatus)
                {
                    case AccountingPeriodStatus.Open:
                        accountingPeriod.Reopen(now);
                        break;
                    case AccountingPeriodStatus.SoftClosed:
                    case AccountingPeriodStatus.Closed:
                        if (targetStatus == AccountingPeriodStatus.SoftClosed &&
                            accountingPeriod.Status == AccountingPeriodStatus.Closed)
                        {
                            throw new FiscalCalendarConflictException(
                                "A closed accounting period must be reopened before it can be soft-closed.");
                        }

                        // Do not certify a closed period with unposted or
                        // misallocated ledger entries. Both reads are made
                        // under the same calendar locks as posting.
                        if (await repository.HasDraftJournalsInPeriodAsync(
                                organizationId, periodId, ct))
                        {
                            throw new FiscalCalendarConflictException(
                                "Resolve draft journal entries before closing the accounting period.");
                        }

                        if (await repository.HasMisdatedJournalsInPeriodAsync(
                                organizationId, periodId,
                                accountingPeriod.StartDate,
                                accountingPeriod.EndDate, ct))
                        {
                            throw new FiscalCalendarConflictException(
                                "The accounting period contains journal entries with posting dates outside its date range.");
                        }

                        if (targetStatus == AccountingPeriodStatus.SoftClosed)
                        {
                            accountingPeriod.SoftClose(now);
                        }
                        else
                        {
                            accountingPeriod.Close(now);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(targetStatus), targetStatus,
                            "Unsupported accounting period status.");
                }

                await repository.SaveChangesAsync(ct);
                return MapPeriod(accountingPeriod);
            },
            cancellationToken);
    }

    private async Task EnsureFiscalYearUniqueAsync(
        Guid organizationId,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingFiscalYearId,
        CancellationToken cancellationToken)
    {
        if (await repository.FiscalYearNameExistsAsync(
                organizationId,
                name,
                excludingFiscalYearId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                $"Fiscal year '{name}' already exists in this organization.");
        }

        if (await repository.FiscalYearRangeOverlapsAsync(
                organizationId,
                startDate,
                endDate,
                excludingFiscalYearId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                "The fiscal year date range overlaps another fiscal year.");
        }
    }

    private async Task EnsurePeriodUniqueAsync(
        Guid organizationId,
        Guid fiscalYearId,
        int number,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingPeriodId,
        CancellationToken cancellationToken)
    {
        if (await repository.PeriodNumberExistsAsync(
                organizationId,
                fiscalYearId,
                number,
                excludingPeriodId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                $"Accounting period number {number} already exists in this fiscal year.");
        }

        if (await repository.PeriodRangeOverlapsAsync(
                organizationId,
                fiscalYearId,
                startDate,
                endDate,
                excludingPeriodId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                "The accounting period date range overlaps another period.");
        }
    }

    private async Task<FiscalYear> RequireOpenFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await repository.GetFiscalYearAsync(
            organizationId,
            fiscalYearId,
            trackChanges: false,
            cancellationToken);

        if (fiscalYear is null)
        {
            throw new FiscalCalendarConflictException(
                "The fiscal year does not exist in this organization.");
        }

        EnsureFiscalYearOpen(fiscalYear);
        return fiscalYear;
    }

    private async Task<(Guid OrganizationId, FiscalYear? FiscalYear)>
        GetTrackedFiscalYearAsync(
            Guid fiscalYearId,
            CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var fiscalYear = await repository.GetFiscalYearAsync(
            organizationId,
            fiscalYearId,
            trackChanges: true,
            cancellationToken);

        return (organizationId, fiscalYear);
    }

    private async Task EnsureFiscalYearHasPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        if (await repository.CountPeriodsAsync(
                organizationId,
                fiscalYearId,
                cancellationToken) == 0)
        {
            throw new FiscalCalendarConflictException(
                "A fiscal year without accounting periods cannot be closed.");
        }
    }

    private static void EnsureFiscalYearOpen(FiscalYear fiscalYear)
    {
        if (fiscalYear.Status != FiscalYearStatus.Open)
        {
            throw new FiscalCalendarConflictException(
                "The fiscal year must be open for this operation.");
        }
    }

    private static void EnsurePeriodInsideFiscalYear(
        FiscalYear fiscalYear,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (!fiscalYear.Contains(startDate, endDate))
        {
            throw new FiscalCalendarConflictException(
                "The accounting period must be fully contained within its fiscal year.");
        }
    }

    private static void ValidateRange(
        DateOnly startDate,
        DateOnly endDate,
        string label)
    {
        if (endDate < startDate)
        {
            throw new FiscalCalendarConflictException(
                $"{label} end date cannot be before its start date.");
        }
    }

    private static FiscalYearResponse MapFiscalYear(FiscalYear fiscalYear)
    {
        return new FiscalYearResponse(
            fiscalYear.Id,
            fiscalYear.OrganizationId,
            fiscalYear.Name,
            fiscalYear.StartDate,
            fiscalYear.EndDate,
            fiscalYear.Status.ToString(),
            fiscalYear.CreatedAtUtc,
            fiscalYear.CreatedByUserId,
            fiscalYear.UpdatedAtUtc,
            fiscalYear.UpdatedByUserId);
    }

    private static AccountingPeriodResponse MapPeriod(AccountingPeriod period)
    {
        return new AccountingPeriodResponse(
            period.Id,
            period.OrganizationId,
            period.FiscalYearId,
            period.Number,
            period.Name,
            period.StartDate,
            period.EndDate,
            period.Status.ToString(),
            period.CreatedAtUtc,
            period.CreatedByUserId,
            period.UpdatedAtUtc,
            period.UpdatedByUserId);
    }
}
