namespace Fintrox.Application.Reports;

public interface IAccountingReportRepository
{
    Task<IReadOnlyList<AccountBalanceMovement>> GetTrialBalanceMovementsAsync(
        Guid organizationId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);

    Task<decimal> GetOpeningNetBalanceAsync(
        Guid organizationId,
        Guid accountId,
        DateOnly fromDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GeneralLedgerMovement>> GetGeneralLedgerMovementsAsync(
        Guid organizationId,
        Guid accountId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);
}
