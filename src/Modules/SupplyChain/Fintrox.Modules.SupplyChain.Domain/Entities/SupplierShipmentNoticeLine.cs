namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierShipmentNoticeLine domain record. No cross-module navigation properties.</summary>
public sealed class SupplierShipmentNoticeLine : OrganizationEntity
{
    public SupplierShipmentNoticeLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentNoticeId,
        Guid purchaseOrderLineId,
        decimal shippedQuantity,
        string inventoryLotReference)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentNoticeId = DomainRules.Id(shipmentNoticeId, nameof(shipmentNoticeId));
        PurchaseOrderLineId = DomainRules.Id(purchaseOrderLineId, nameof(purchaseOrderLineId));
        ShippedQuantity = DomainRules.Positive(shippedQuantity, nameof(shippedQuantity));
        InventoryLotReference = DomainRules.Text(inventoryLotReference, nameof(inventoryLotReference));
    }

    public Guid ShipmentNoticeId { get; }
    public Guid PurchaseOrderLineId { get; }
    public decimal ShippedQuantity { get; }
    public string InventoryLotReference { get; }
}
