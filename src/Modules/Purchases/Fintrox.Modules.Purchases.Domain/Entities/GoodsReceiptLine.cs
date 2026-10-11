namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class GoodsReceiptLine : OrganizationEntity
{
    public GoodsReceiptLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid goodsReceiptId,
        Guid purchaseOrderLineId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        GoodsReceiptId = DomainRules.Id(goodsReceiptId, nameof(goodsReceiptId));
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid GoodsReceiptId { get; }
    public Guid PurchaseOrderLineId { get; }
    public decimal Quantity { get; }
}
