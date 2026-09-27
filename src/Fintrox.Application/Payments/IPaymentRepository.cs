using Fintrox.Domain.Payments;

namespace Fintrox.Application.Payments;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> ListPaymentsAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        PaymentStatus? status,
        PaymentDirection? direction,
        Guid? counterpartyId,
        CancellationToken cancellationToken);

    Task<Payment?> GetPaymentAsync(
        Guid organizationId,
        Guid paymentId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentAllocation>> ListAllocationsAsync(
        Guid organizationId,
        Guid paymentId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<PaymentAllocation?> GetAllocationAsync(
        Guid organizationId,
        Guid paymentId,
        Guid allocationId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<int> GetNextAllocationLineNumberAsync(
        Guid organizationId,
        Guid paymentId,
        CancellationToken cancellationToken);

    Task<decimal> GetConfirmedSalesInvoiceAllocatedAmountAsync(
        Guid organizationId,
        Guid salesInvoiceId,
        Guid? excludingPaymentId,
        CancellationToken cancellationToken);

    Task<decimal> GetConfirmedPurchaseDocumentAllocatedAmountAsync(
        Guid organizationId,
        Guid purchaseDocumentId,
        Guid? excludingPaymentId,
        CancellationToken cancellationToken);

    Task LockSalesInvoiceAsync(
        Guid organizationId,
        Guid salesInvoiceId,
        CancellationToken cancellationToken);

    Task LockPurchaseDocumentAsync(
        Guid organizationId,
        Guid purchaseDocumentId,
        CancellationToken cancellationToken);

    Task<long> AllocatePaymentSequenceAsync(
        Guid organizationId,
        int calendarYear,
        PaymentDirection direction,
        CancellationToken cancellationToken);

    Task AddPaymentAsync(
        Payment payment,
        CancellationToken cancellationToken);

    Task AddAllocationAsync(
        PaymentAllocation allocation,
        CancellationToken cancellationToken);

    void RemoveAllocation(PaymentAllocation allocation);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
