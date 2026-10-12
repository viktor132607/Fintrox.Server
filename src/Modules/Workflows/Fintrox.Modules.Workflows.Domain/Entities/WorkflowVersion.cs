namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowVersion; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowVersion : OrganizationEntity
{
    public WorkflowVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowDefinitionId,
        int versionNumber,
        DateTimeOffset publishedAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowDefinitionId = DomainRules.Id(workflowDefinitionId, nameof(workflowDefinitionId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        PublishedAtUtc = DomainRules.Instant(publishedAtUtc, nameof(publishedAtUtc));
        IsActive = isActive;
    }

    public Guid WorkflowDefinitionId { get; }
    public int VersionNumber { get; }
    public DateTimeOffset PublishedAtUtc { get; }
    public bool IsActive { get; }
}
