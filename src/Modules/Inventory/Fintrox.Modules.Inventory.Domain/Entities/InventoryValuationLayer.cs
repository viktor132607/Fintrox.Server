namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryValuationLayer domain record; references checked in application layer.</summary>
public sealed class InventoryValuationLayer : OrganizationEntity
{
    public InventoryValuationLayer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid warehouseId,
        Guid receiptReferenceId,
        decimal remainingQuantity,
        decimal unitCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ReceiptReferenceId = DomainRules.Id(receiptReferenceId, nameof(receiptReferenceId));
        RemainingQuantity = DomainRules.Nonnegative(remainingQuantity, nameof(remainingQuantity));
        UnitCost = DomainRules.Nonnegative(unitCost, nameof(unitCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid InventoryItemId { get; }
    public Guid WarehouseId { get; }
    public Guid ReceiptReferenceId { get; }
    public decimal RemainingQuantity { get; }
    public decimal UnitCost { get; }
    public string Currency { get; }
}
