namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryLotAttribute domain record; references checked in application layer.</summary>
public sealed class InventoryLotAttribute : OrganizationEntity
{
    public InventoryLotAttribute(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryLotId,
        string attributeCode,
        string attributeValue,
        DateOnly recordedOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        AttributeCode = DomainRules.Text(attributeCode, nameof(attributeCode));
        AttributeValue = DomainRules.Text(attributeValue, nameof(attributeValue));
        RecordedOn = DomainRules.Date(recordedOn, nameof(recordedOn));
    }

    public Guid InventoryLotId { get; }
    public string AttributeCode { get; }
    public string AttributeValue { get; }
    public DateOnly RecordedOn { get; }
}
