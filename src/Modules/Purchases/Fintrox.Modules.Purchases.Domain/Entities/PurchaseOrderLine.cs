namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class PurchaseOrderLine : OrganizationEntity
{
    public PurchaseOrderLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PurchaseOrderId { get; }
    public Guid ItemId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
