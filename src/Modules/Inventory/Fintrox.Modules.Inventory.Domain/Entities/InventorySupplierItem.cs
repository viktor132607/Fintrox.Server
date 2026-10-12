namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventorySupplierItem domain record; references checked in application layer.</summary>
public sealed class InventorySupplierItem : OrganizationEntity
{
    public InventorySupplierItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid supplierId,
        string supplierSku,
        decimal minimumOrderQuantity,
        int leadTimeDays)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        SupplierSku = DomainRules.Text(supplierSku, nameof(supplierSku));
        MinimumOrderQuantity = DomainRules.Positive(minimumOrderQuantity, nameof(minimumOrderQuantity));
        LeadTimeDays = DomainRules.PositiveInt(leadTimeDays, nameof(leadTimeDays));
    }

    public Guid InventoryItemId { get; }
    public Guid SupplierId { get; }
    public string SupplierSku { get; }
    public decimal MinimumOrderQuantity { get; }
    public int LeadTimeDays { get; }
}
