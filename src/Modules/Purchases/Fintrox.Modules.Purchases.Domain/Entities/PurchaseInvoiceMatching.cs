namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseInvoiceMatching model. References require application validation.</summary>
public sealed class PurchaseInvoiceMatching : OrganizationEntity
{
    public PurchaseInvoiceMatching(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierInvoiceId,
        Guid purchaseOrderId,
        Guid goodsReceiptId,
        DateTimeOffset matchedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierInvoiceId = DomainRules.Id(supplierInvoiceId, nameof(supplierInvoiceId));
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        GoodsReceiptId = DomainRules.Id(goodsReceiptId, nameof(goodsReceiptId));
        MatchedAtUtc = DomainRules.Instant(matchedAtUtc, nameof(matchedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SupplierInvoiceId { get; }
    public Guid PurchaseOrderId { get; }
    public Guid GoodsReceiptId { get; }
    public DateTimeOffset MatchedAtUtc { get; }
    public string Status { get; }
}
