namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventorySku domain record; references checked in application layer.</summary>
public sealed class InventorySku : OrganizationEntity
{
    public InventorySku(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        string sku,
        string barcode,
        bool isStockTracked)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        Sku = DomainRules.Text(sku, nameof(sku));
        Barcode = DomainRules.Text(barcode, nameof(barcode));
        IsStockTracked = isStockTracked;
    }

    public Guid InventoryItemId { get; }
    public string Sku { get; }
    public string Barcode { get; }
    public bool IsStockTracked { get; }
}
