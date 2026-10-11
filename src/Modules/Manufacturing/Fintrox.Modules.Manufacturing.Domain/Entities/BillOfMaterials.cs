namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class BillOfMaterials : OrganizationEntity
{
    public BillOfMaterials(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        string revision,
        decimal outputQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Revision = DomainRules.Text(revision, nameof(revision));
        OutputQuantity = DomainRules.Positive(outputQuantity, nameof(outputQuantity));
    }

    public Guid ItemId { get; }
    public string Revision { get; }
    public decimal OutputQuantity { get; }
}
