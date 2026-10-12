namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseCrossDockAllocation; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseCrossDockAllocation : OrganizationEntity
{
    public WarehouseCrossDockAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid receivingTaskId,
        Guid shippingTaskId,
        Guid inventoryItemId,
        decimal allocatedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        ReceivingTaskId = DomainRules.Id(receivingTaskId, nameof(receivingTaskId));
        ShippingTaskId = DomainRules.Id(shippingTaskId, nameof(shippingTaskId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        AllocatedQuantity = DomainRules.Positive(allocatedQuantity, nameof(allocatedQuantity));
    }

    public Guid ReceivingTaskId { get; }
    public Guid ShippingTaskId { get; }
    public Guid InventoryItemId { get; }
    public decimal AllocatedQuantity { get; }
}
