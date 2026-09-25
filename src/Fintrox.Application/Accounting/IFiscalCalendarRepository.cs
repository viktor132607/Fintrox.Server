using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public interface IFiscalCalendarRepository
{
    Task<IReadOnlyList<FiscalYear>> ListFiscalYearsAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<FiscalYear?> GetFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> FiscalYearNameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingFiscalYearId,
        CancellationToken cancellationToken);

    Task<bool> FiscalYearRangeOverlapsAsync(
        Guid organizationId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingFiscalYearId,
        CancellationToken cancellationToken);

    Task<bool> HasPeriodOutsideRangeAsync(
        Guid organizationId,
        Guid fiscalYearId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccountingPeriod>> ListPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<AccountingPeriod?> GetPeriodAsync(
        Guid organizationId,
        Guid fiscalYearId,
        Guid periodId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<AccountingPeriod?> FindPeriodByDateAsync(
        Guid organizationId,
        DateOnly postingDate,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> PeriodNumberExistsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        int number,
        Guid? excludingPeriodId,
        CancellationToken cancellationToken);

    Task<bool> PeriodRangeOverlapsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingPeriodId,
        CancellationToken cancellationToken);

    Task<int> CountPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<bool> HasOpenPeriodsAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<bool> AllPeriodsClosedAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task AddFiscalYearAsync(
        FiscalYear fiscalYear,
        CancellationToken cancellationToken);

    Task AddPeriodAsync(
        AccountingPeriod period,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
