namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class SalesPipeline : OrganizationEntity
{
    public SalesPipeline(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
    }

    public string Name { get; }
}
