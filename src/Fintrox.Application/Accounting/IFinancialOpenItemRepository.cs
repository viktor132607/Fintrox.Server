namespace Fintrox.Application.Accounting;

public interface IFinancialOpenItemRepository
{
    Task<IReadOnlyList<FinancialOpenDocumentSnapshot>> GetDocumentsAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialPaymentHeaderSnapshot>> GetPaymentsAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialOpenAllocationSnapshot>> GetAllocationsAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialDocumentControlSnapshot>> GetOriginalControlLinesAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialControlLedgerSnapshot>> GetControlLedgerBalancesAsync(
        Guid organizationId, DateOnly cutoff, IReadOnlyList<Guid> accountIds,
        CancellationToken cancellationToken);
}
