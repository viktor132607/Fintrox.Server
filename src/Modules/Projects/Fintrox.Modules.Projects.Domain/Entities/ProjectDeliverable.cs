namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectDeliverable; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectDeliverable : OrganizationEntity
{
    public ProjectDeliverable(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid milestoneId,
        string name,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        MilestoneId = DomainRules.Id(milestoneId, nameof(milestoneId));
        Name = DomainRules.Text(name, nameof(name));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ProjectId { get; }
    public Guid MilestoneId { get; }
    public string Name { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
