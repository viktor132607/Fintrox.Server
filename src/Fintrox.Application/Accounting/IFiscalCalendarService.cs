using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IFiscalCalendarService
{
    Task<IReadOnlyList<FiscalYearResponse>> ListFiscalYearsAsync(
        CancellationToken cancellationToken);

    Task<FiscalYearResponse?> GetFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<FiscalYearResponse> CreateFiscalYearAsync(
        CreateFiscalYearRequest request,
        CancellationToken cancellationToken);

    Task<FiscalYearResponse?> UpdateFiscalYearAsync(
        Guid fiscalYearId,
        UpdateFiscalYearRequest request,
        CancellationToken cancellationToken);

    Task<FiscalYearResponse?> SoftCloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<FiscalYearResponse?> CloseFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<FiscalYearResponse?> ReopenFiscalYearAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccountingPeriodResponse>> ListPeriodsAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse?> GetPeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse> CreatePeriodAsync(
        Guid fiscalYearId,
        CreateAccountingPeriodRequest request,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse?> UpdatePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        UpdateAccountingPeriodRequest request,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse?> SoftClosePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse?> ClosePeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken);

    Task<AccountingPeriodResponse?> ReopenPeriodAsync(
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken);
}
