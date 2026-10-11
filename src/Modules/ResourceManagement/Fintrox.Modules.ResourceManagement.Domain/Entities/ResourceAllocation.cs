namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class ResourceAllocation : OrganizationEntity
{
    public ResourceAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        Guid projectId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        decimal allocationPercent)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        AllocationPercent = DomainRules.Percent(allocationPercent, nameof(allocationPercent));
        if (EndsAtUtc < StartsAtUtc) throw new ArgumentException("The end cannot precede the start.", nameof(endsAtUtc));
    }

    public Guid ResourceId { get; }
    public Guid ProjectId { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
    public decimal AllocationPercent { get; }
}
