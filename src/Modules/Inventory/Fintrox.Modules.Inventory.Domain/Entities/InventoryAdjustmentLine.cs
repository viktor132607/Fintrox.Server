namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAdjustmentLine domain record; references checked in application layer.</summary>
public sealed class InventoryAdjustmentLine : OrganizationEntity
{
    public InventoryAdjustmentLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryAdjustmentId,
        Guid inventoryItemId,
        decimal quantityChange,
        decimal unitCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryAdjustmentId = DomainRules.Id(inventoryAdjustmentId, nameof(inventoryAdjustmentId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        QuantityChange = DomainRules.Nonzero(quantityChange, nameof(quantityChange));
        UnitCost = DomainRules.Nonnegative(unitCost, nameof(unitCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid InventoryAdjustmentId { get; }
    public Guid InventoryItemId { get; }
    public decimal QuantityChange { get; }
    public decimal UnitCost { get; }
    public string Currency { get; }
}
