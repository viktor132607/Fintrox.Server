using Fintrox.Contracts.Payments;

namespace Fintrox.Application.Payments;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentSummaryResponse>> ListAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        string? direction,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<PaymentResponse?> GetAsync(
        Guid paymentId,
        CancellationToken cancellationToken);

    Task<PaymentResponse> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken);

    Task<PaymentResponse?> UpdateAsync(
        Guid paymentId,
        UpdatePaymentRequest request,
        CancellationToken cancellationToken);

    Task<PaymentAllocationResponse?> AddAllocationAsync(
        Guid paymentId,
        CreatePaymentAllocationRequest request,
        CancellationToken cancellationToken);

    Task<PaymentAllocationResponse?> UpdateAllocationAsync(
        Guid paymentId,
        Guid allocationId,
        UpdatePaymentAllocationRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAllocationAsync(
        Guid paymentId,
        Guid allocationId,
        CancellationToken cancellationToken);

    Task<PaymentResponse?> ConfirmAsync(
        Guid paymentId,
        CancellationToken cancellationToken);

    Task<PaymentResponse?> CancelAsync(
        Guid paymentId,
        CancelPaymentRequest request,
        CancellationToken cancellationToken);

    Task<DocumentSettlementResponse?> GetSalesInvoiceSettlementAsync(
        Guid salesInvoiceId,
        CancellationToken cancellationToken);

    Task<DocumentSettlementResponse?> GetPurchaseDocumentSettlementAsync(
        Guid purchaseDocumentId,
        CancellationToken cancellationToken);
}
