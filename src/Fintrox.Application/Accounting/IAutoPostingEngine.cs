using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;

namespace Fintrox.Application.Accounting;

public interface IAutoPostingEngine
{
    Task PostSalesInvoiceAsync(
        SalesInvoice invoice,
        IReadOnlyList<SalesInvoiceLine> lines,
        CancellationToken cancellationToken);

    Task ReverseSalesInvoiceAsync(
        SalesInvoice invoice,
        string reason,
        CancellationToken cancellationToken);

    Task PostPurchaseDocumentAsync(
        PurchaseDocument document,
        IReadOnlyList<PurchaseDocumentLine> lines,
        CancellationToken cancellationToken);

    Task ReversePurchaseDocumentAsync(
        PurchaseDocument document,
        string reason,
        CancellationToken cancellationToken);

    Task PostPaymentAsync(
        Payment payment,
        IReadOnlyList<PaymentAllocation> allocations,
        CancellationToken cancellationToken);

    Task ReversePaymentAsync(
        Payment payment,
        string reason,
        CancellationToken cancellationToken);
}
