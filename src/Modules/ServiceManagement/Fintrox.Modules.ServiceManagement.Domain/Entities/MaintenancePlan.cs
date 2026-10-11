namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

public sealed class MaintenancePlan : OrganizationEntity
{
    public MaintenancePlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid assetId,
        string name,
        int intervalDays,
        DateOnly nextDueOn)
        : base(id, organizationId, createdAtUtc)
    {
        AssetId = DomainRules.Id(assetId, nameof(assetId));
        Name = DomainRules.Text(name, nameof(name));
        IntervalDays = DomainRules.PositiveInt(intervalDays, nameof(intervalDays));
        NextDueOn = DomainRules.Date(nextDueOn, nameof(nextDueOn));
    }

    public Guid AssetId { get; }
    public string Name { get; }
    public int IntervalDays { get; }
    public DateOnly NextDueOn { get; }
}
