namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class Resource : OrganizationEntity
{
    public Resource(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        decimal capacityHoursPerDay)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        CapacityHoursPerDay = DomainRules.Hours(capacityHoursPerDay, nameof(capacityHoursPerDay));
    }

    public string Code { get; }
    public string Name { get; }
    public decimal CapacityHoursPerDay { get; }
}
