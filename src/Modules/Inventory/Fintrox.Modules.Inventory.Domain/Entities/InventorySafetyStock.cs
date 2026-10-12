namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventorySafetyStock domain record; references checked in application layer.</summary>
public sealed class InventorySafetyStock : OrganizationEntity
{
    public InventorySafetyStock(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        decimal safetyQuantity,
        DateOnly calculatedOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        SafetyQuantity = DomainRules.Nonnegative(safetyQuantity, nameof(safetyQuantity));
        CalculatedOn = DomainRules.Date(calculatedOn, nameof(calculatedOn));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public decimal SafetyQuantity { get; }
    public DateOnly CalculatedOn { get; }
}
