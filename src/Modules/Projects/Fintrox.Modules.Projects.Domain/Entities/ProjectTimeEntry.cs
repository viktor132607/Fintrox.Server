namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectTimeEntry; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectTimeEntry : OrganizationEntity
{
    public ProjectTimeEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid projectTaskId,
        Guid resourceId,
        DateOnly workDate,
        decimal hours)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        ProjectTaskId = DomainRules.Id(projectTaskId, nameof(projectTaskId));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        WorkDate = DomainRules.Date(workDate, nameof(workDate));
        Hours = DomainRules.Positive(hours, nameof(hours));
    }

    public Guid ProjectId { get; }
    public Guid ProjectTaskId { get; }
    public Guid ResourceId { get; }
    public DateOnly WorkDate { get; }
    public decimal Hours { get; }
}
