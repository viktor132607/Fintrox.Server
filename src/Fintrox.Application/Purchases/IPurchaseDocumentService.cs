using Fintrox.Contracts.Purchases;

namespace Fintrox.Application.Purchases;

public interface IPurchaseDocumentService
{
    Task<IReadOnlyList<PurchaseDocumentSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? type,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentResponse?> GetAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentResponse> CreateAsync(
        CreatePurchaseDocumentRequest request,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentResponse?> UpdateAsync(
        Guid documentId,
        UpdatePurchaseDocumentRequest request,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentLineResponse?> AddLineAsync(
        Guid documentId,
        CreatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentLineResponse?> UpdateLineAsync(
        Guid documentId,
        Guid lineId,
        UpdatePurchaseDocumentLineRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteLineAsync(
        Guid documentId,
        Guid lineId,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentResponse?> ReceiveAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<PurchaseDocumentResponse?> CancelAsync(
        Guid documentId,
        CancelPurchaseDocumentRequest request,
        CancellationToken cancellationToken);
}
