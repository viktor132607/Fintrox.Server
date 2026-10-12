namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAvailabilitySnapshot domain record; references checked in application layer.</summary>
public sealed class InventoryAvailabilitySnapshot : OrganizationEntity
{
    public InventoryAvailabilitySnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        decimal availableQuantity,
        decimal reservedQuantity,
        DateTimeOffset asOfUtc)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        AvailableQuantity = DomainRules.Nonnegative(availableQuantity, nameof(availableQuantity));
        ReservedQuantity = DomainRules.Nonnegative(reservedQuantity, nameof(reservedQuantity));
        AsOfUtc = DomainRules.Instant(asOfUtc, nameof(asOfUtc));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public decimal AvailableQuantity { get; }
    public decimal ReservedQuantity { get; }
    public DateTimeOffset AsOfUtc { get; }
}
