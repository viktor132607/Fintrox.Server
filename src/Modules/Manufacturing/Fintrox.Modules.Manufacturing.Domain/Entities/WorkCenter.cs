namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class WorkCenter : OrganizationEntity
{
    public WorkCenter(
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
