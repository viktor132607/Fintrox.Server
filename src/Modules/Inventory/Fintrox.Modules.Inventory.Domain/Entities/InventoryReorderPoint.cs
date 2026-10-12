namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryReorderPoint domain record; references checked in application layer.</summary>
public sealed class InventoryReorderPoint : OrganizationEntity
{
    public InventoryReorderPoint(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        decimal minimumQuantity,
        decimal targetQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        MinimumQuantity = DomainRules.Nonnegative(minimumQuantity, nameof(minimumQuantity));
        TargetQuantity = DomainRules.Nonnegative(targetQuantity, nameof(targetQuantity));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public decimal MinimumQuantity { get; }
    public decimal TargetQuantity { get; }
}
