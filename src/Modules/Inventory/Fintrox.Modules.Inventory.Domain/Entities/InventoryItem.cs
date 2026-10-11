namespace Fintrox.Modules.Inventory.Domain.Entities;

public sealed class InventoryItem : OrganizationEntity
{
    public InventoryItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string sku,
        string name,
        Guid unitOfMeasureId)
        : base(id, organizationId, createdAtUtc)
    {
        Sku = DomainRules.Text(sku, nameof(sku));
        Name = DomainRules.Text(name, nameof(name));
        UnitOfMeasureId = DomainRules.Id(unitOfMeasureId, nameof(unitOfMeasureId));
    }

    public string Sku { get; }
    public string Name { get; }
    public Guid UnitOfMeasureId { get; }
}
