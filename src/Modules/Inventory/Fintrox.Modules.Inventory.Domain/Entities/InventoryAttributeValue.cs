namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryAttributeValue domain record; references checked in application layer.</summary>
public sealed class InventoryAttributeValue : OrganizationEntity
{
    public InventoryAttributeValue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid inventoryAttributeId,
        string value,
        bool isSearchable)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        InventoryAttributeId = DomainRules.Id(inventoryAttributeId, nameof(inventoryAttributeId));
        Value = DomainRules.Text(value, nameof(value));
        IsSearchable = isSearchable;
    }

    public Guid InventoryItemId { get; }
    public Guid InventoryAttributeId { get; }
    public string Value { get; }
    public bool IsSearchable { get; }
}
