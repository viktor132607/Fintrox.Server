using Fintrox.Domain.Purchases;

namespace Fintrox.Application.Purchases;

public interface IPurchaseDocumentRepository
{
    Task<IReadOnlyList<PurchaseDocument>> ListAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        PurchaseDocumentStatus? status,
        PurchaseDocumentType? type,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<PurchaseDocument?> GetAsync(
        Guid organizationId,
        Guid documentId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PurchaseDocumentLine>> ListLinesAsync(
        Guid organizationId,
        Guid documentId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentLine?> GetLineAsync(
        Guid organizationId,
        Guid documentId,
        Guid lineId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid documentId,
        CancellationToken cancellationToken);

    Task<bool> SupplierDocumentNumberExistsAsync(
        Guid organizationId,
        Guid counterpartyId,
        string supplierDocumentNumber,
        Guid? excludingDocumentId,
        CancellationToken cancellationToken);

    Task<long> AllocateInternalSequenceAsync(
        Guid organizationId,
        int calendarYear,
        CancellationToken cancellationToken);

    Task AddAsync(
        PurchaseDocument document,
        CancellationToken cancellationToken);

    Task AddLineAsync(
        PurchaseDocumentLine line,
        CancellationToken cancellationToken);

    void RemoveLine(PurchaseDocumentLine line);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
