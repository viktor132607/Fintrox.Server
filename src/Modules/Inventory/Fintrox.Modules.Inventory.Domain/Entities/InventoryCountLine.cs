namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryCountLine domain record; references checked in application layer.</summary>
public sealed class InventoryCountLine : OrganizationEntity
{
    public InventoryCountLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryCountId,
        Guid inventoryItemId,
        decimal expectedQuantity,
        decimal countedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryCountId = DomainRules.Id(inventoryCountId, nameof(inventoryCountId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ExpectedQuantity = DomainRules.Nonnegative(expectedQuantity, nameof(expectedQuantity));
        CountedQuantity = DomainRules.Nonnegative(countedQuantity, nameof(countedQuantity));
    }

    public Guid InventoryCountId { get; }
    public Guid InventoryItemId { get; }
    public decimal ExpectedQuantity { get; }
    public decimal CountedQuantity { get; }
}
