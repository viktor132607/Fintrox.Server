namespace Fintrox.Modules.Inventory.Domain.Entities;

public sealed class InventoryLot : OrganizationEntity
{
    public InventoryLot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        string lotNumber,
        DateOnly expiresOn)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        LotNumber = DomainRules.Text(lotNumber, nameof(lotNumber));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
    }

    public Guid ItemId { get; }
    public string LotNumber { get; }
    public DateOnly ExpiresOn { get; }
}
