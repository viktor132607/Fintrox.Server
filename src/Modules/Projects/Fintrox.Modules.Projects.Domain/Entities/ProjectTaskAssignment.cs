namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectTaskAssignment; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectTaskAssignment : OrganizationEntity
{
    public ProjectTaskAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectTaskId,
        Guid resourceId,
        decimal allocationPercent,
        DateOnly assignedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectTaskId = DomainRules.Id(projectTaskId, nameof(projectTaskId));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        AllocationPercent = DomainRules.Percent(allocationPercent, nameof(allocationPercent));
        AssignedOn = DomainRules.Date(assignedOn, nameof(assignedOn));
    }

    public Guid ProjectTaskId { get; }
    public Guid ResourceId { get; }
    public decimal AllocationPercent { get; }
    public DateOnly AssignedOn { get; }
}
