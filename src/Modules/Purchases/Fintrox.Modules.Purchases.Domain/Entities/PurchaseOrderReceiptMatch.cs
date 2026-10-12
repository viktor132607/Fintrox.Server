namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseOrderReceiptMatch model. References require application validation.</summary>
public sealed class PurchaseOrderReceiptMatch : OrganizationEntity
{
    public PurchaseOrderReceiptMatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderLineId,
        Guid goodsReceiptLineId,
        decimal matchedQuantity,
        DateTimeOffset matchedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        GoodsReceiptLineId = DomainRules.Id(goodsReceiptLineId, nameof(goodsReceiptLineId));
        MatchedQuantity = DomainRules.Positive(matchedQuantity, nameof(matchedQuantity));
        MatchedAtUtc = DomainRules.Instant(matchedAtUtc, nameof(matchedAtUtc));
    }

    public Guid PurchaseOrderLineId { get; }
    public Guid GoodsReceiptLineId { get; }
    public decimal MatchedQuantity { get; }
    public DateTimeOffset MatchedAtUtc { get; }
}
