namespace Fintrox.Application.Accounting;

/// <summary>Payment-currency allocation amount from an existing document link.</summary>
public sealed record FinancialPaymentAllocationSnapshot(
    Guid PaymentId, decimal PaymentAmount, decimal DocumentAmount,
    Guid? SalesInvoiceId, Guid? PurchaseDocumentId);
