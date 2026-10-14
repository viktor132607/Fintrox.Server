namespace Fintrox.Application.Accounting;

public interface IFinancialSourceReconciliationRepository
{
    Task<IReadOnlyList<FinancialSourcePostingSnapshot>> ListFinancialSourcesAsync(
        Guid organizationId, DateOnly from, DateOnly to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialSourceJournalSnapshot>> ListSourceJournalsAsync(
        Guid organizationId, DateOnly from, DateOnly to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialPaymentAllocationSnapshot>> ListPaymentAllocationsAsync(
        Guid organizationId, DateOnly from, DateOnly to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialPostedReversalSnapshot>> GetPostedReversalsAsync(
        Guid organizationId, IReadOnlyList<Guid> originalIds,
        CancellationToken cancellationToken);
}
