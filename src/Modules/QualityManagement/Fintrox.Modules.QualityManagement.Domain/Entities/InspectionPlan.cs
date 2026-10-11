namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class InspectionPlan : OrganizationEntity
{
    public InspectionPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        string name)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Name = DomainRules.Text(name, nameof(name));
    }

    public Guid ItemId { get; }
    public string Name { get; }
}
