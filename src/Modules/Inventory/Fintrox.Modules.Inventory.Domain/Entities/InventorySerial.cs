namespace Fintrox.Modules.Inventory.Domain.Entities;

public sealed class InventorySerial : OrganizationEntity
{
    public InventorySerial(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        string serialNumber)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        SerialNumber = DomainRules.Text(serialNumber, nameof(serialNumber));
    }

    public Guid ItemId { get; }
    public string SerialNumber { get; }
}
