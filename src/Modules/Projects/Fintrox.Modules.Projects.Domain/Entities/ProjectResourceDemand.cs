namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectResourceDemand; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectResourceDemand : OrganizationEntity
{
    public ProjectResourceDemand(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string resourceSkillCode,
        decimal requiredHours,
        DateOnly neededBy)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        ResourceSkillCode = DomainRules.Text(resourceSkillCode, nameof(resourceSkillCode));
        RequiredHours = DomainRules.Positive(requiredHours, nameof(requiredHours));
        NeededBy = DomainRules.Date(neededBy, nameof(neededBy));
    }

    public Guid ProjectId { get; }
    public string ResourceSkillCode { get; }
    public decimal RequiredHours { get; }
    public DateOnly NeededBy { get; }
}
