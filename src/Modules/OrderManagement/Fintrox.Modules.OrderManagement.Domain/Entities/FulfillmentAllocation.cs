namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned FulfillmentAllocation record.</summary>
public sealed class FulfillmentAllocation : OrganizationEntity
{
    public FulfillmentAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentLineId,
        Guid warehouseId,
        Guid inventoryLotId,
        decimal allocatedQuantity,
        DateTimeOffset allocatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentLineId = DomainRules.Id(fulfillmentLineId, nameof(fulfillmentLineId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        AllocatedQuantity = DomainRules.Positive(allocatedQuantity, nameof(allocatedQuantity));
        AllocatedAtUtc = DomainRules.Instant(allocatedAtUtc, nameof(allocatedAtUtc));
    }

    public Guid FulfillmentLineId { get; }
    public Guid WarehouseId { get; }
    public Guid InventoryLotId { get; }
    public decimal AllocatedQuantity { get; }
    public DateTimeOffset AllocatedAtUtc { get; }
}
