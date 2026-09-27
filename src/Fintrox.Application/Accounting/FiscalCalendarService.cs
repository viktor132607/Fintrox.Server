using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public sealed class FiscalCalendarService(
    IFiscalCalendarRepository repository,
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
        var fiscalYear = await repository.GetFiscalYearAsync(
            organizationId,
            fiscalYearId,
            trackChanges: true,
            cancellationToken);

        if (fiscalYear is null)
        {
            return null;
        }

        EnsureFiscalYearOpen(fiscalYear);

        var normalizedName = request.Name.Trim();

        await EnsureFiscalYearUniqueAsync(
            organizationId,
            normalizedName,
            request.StartDate,
            request.EndDate,
            fiscalYearId,
            cancellationToken);

        if (await repository.HasPeriodOutsideRangeAsync(
                organizationId,
                fiscalYearId,
                request.StartDate,
                request.EndDate,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                "The fiscal year cannot exclude existing accounting periods.");
        }

        fiscalYear.Update(
            normalizedName,
            request.StartDate,
            request.EndDate,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return MapFiscalYear(fiscalYear);
    }

    public async Task<FiscalYearResponse?> SoftCloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var (organizationId, fiscalYear) = await GetTrackedFiscalYearAsync(
            fiscalYearId,
            cancellationToken);

        if (fiscalYear is null)
        {
            return null;
        }

        if (fiscalYear.Status == FiscalYearStatus.Closed)
        {
            throw new FiscalCalendarConflictException(
                "A closed fiscal year must be reopened before it can be soft-closed.");
        }

        await EnsureFiscalYearHasPeriodsAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        if (await repository.HasOpenPeriodsAsync(
                organizationId,
                fiscalYearId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                "All accounting periods must be soft-closed or closed before the fiscal year can be soft-closed.");
        }

        fiscalYear.SoftClose(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return MapFiscalYear(fiscalYear);
    }

    public async Task<FiscalYearResponse?> CloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var (organizationId, fiscalYear) = await GetTrackedFiscalYearAsync(
            fiscalYearId,
            cancellationToken);

        if (fiscalYear is null)
        {
            return null;
        }

        await EnsureFiscalYearHasPeriodsAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        if (!await repository.AllPeriodsClosedAsync(
                organizationId,
                fiscalYearId,
                cancellationToken))
        {
            throw new FiscalCalendarConflictException(
                "All accounting periods must be closed before the fiscal year can be closed.");
        }

        fiscalYear.Close(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return MapFiscalYear(fiscalYear);
    }

    public async Task<FiscalYearResponse?> ReopenFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        var (_, fiscalYear) = await GetTrackedFiscalYearAsync(
            fiscalYearId,
            cancellationToken);

        if (fiscalYear is null)
        {
            return null;
        }

        fiscalYear.Reopen(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return MapFiscalYear(fiscalYear);
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
        var fiscalYear = await RequireOpenFiscalYearAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        EnsurePeriodInsideFiscalYear(
            fiscalYear,
            request.StartDate,
            request.EndDate);

        await EnsurePeriodUniqueAsync(
            organizationId,
            fiscalYearId,
            request.Number,
            request.StartDate,
            request.EndDate,
            excludingPeriodId: null,
            cancellationToken);

        var period = AccountingPeriod.Create(
            organizationId,
            fiscalYearId,
            request.Number,
            request.Name,
            request.StartDate,
            request.EndDate,
            timeProvider.GetUtcNow());

        await repository.AddPeriodAsync(period, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return MapPeriod(period);
    }

    public async Task<AccountingPeriodResponse?> UpdatePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        UpdateAccountingPeriodRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRange(request.StartDate, request.EndDate, "Accounting period");

        var organizationId = currentOrganization.RequireOrganizationId();
        var fiscalYear = await RequireOpenFiscalYearAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        var period = await repository.GetPeriodAsync(
            organizationId,
            fiscalYearId,
            periodId,
            trackChanges: true,
            cancellationToken);

        if (period is null)
        {
            return null;
        }

        if (period.Status != AccountingPeriodStatus.Open)
        {
            throw new FiscalCalendarConflictException(
                "Only an open accounting period can be modified.");
        }

        EnsurePeriodInsideFiscalYear(
            fiscalYear,
            request.StartDate,
            request.EndDate);

        await EnsurePeriodUniqueAsync(
            organizationId,
            fiscalYearId,
            request.Number,
            request.StartDate,
            request.EndDate,
            periodId,
            cancellationToken);

        period.Update(
            request.Number,
            request.Name,
            request.StartDate,
            request.EndDate,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return MapPeriod(period);
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
        await RequireOpenFiscalYearAsync(
            organizationId,
            fiscalYearId,
            cancellationToken);

        var period = await repository.GetPeriodAsync(
            organizationId,
            fiscalYearId,
            periodId,
            trackChanges: true,
            cancellationToken);

        if (period is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();

        switch (targetStatus)
        {
            case AccountingPeriodStatus.Open:
                period.Reopen(now);
                break;
            case AccountingPeriodStatus.SoftClosed:
                if (period.Status == AccountingPeriodStatus.Closed)
                {
                    throw new FiscalCalendarConflictException(
                        "A closed accounting period must be reopened before it can be soft-closed.");
                }

                period.SoftClose(now);
                break;
            case AccountingPeriodStatus.Closed:
                period.Close(now);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(targetStatus),
                    targetStatus,
                    "Unsupported accounting period status.");
        }

        await repository.SaveChangesAsync(cancellationToken);
        return MapPeriod(period);
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
