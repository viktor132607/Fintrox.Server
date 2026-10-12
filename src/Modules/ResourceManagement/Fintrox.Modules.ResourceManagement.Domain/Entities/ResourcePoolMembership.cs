namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourcePoolMembership model; no cross-module navigations.</summary>
public sealed class ResourcePoolMembership : OrganizationEntity
{
    public ResourcePoolMembership(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourcePoolId,
        Guid resourceId,
        DateOnly joinedOn,
        decimal allocationPercent)
        : base(id, organizationId, createdAtUtc)
    {
        ResourcePoolId = DomainRules.Id(resourcePoolId, nameof(resourcePoolId));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
        AllocationPercent = DomainRules.Percent(allocationPercent, nameof(allocationPercent));
    }

    public Guid ResourcePoolId { get; }
    public Guid ResourceId { get; }
    public DateOnly JoinedOn { get; }
    public decimal AllocationPercent { get; }
}
