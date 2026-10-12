namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectIssue; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectIssue : OrganizationEntity
{
    public ProjectIssue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string issueCode,
        string description,
        string severity,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        IssueCode = DomainRules.Text(issueCode, nameof(issueCode));
        Description = DomainRules.Text(description, nameof(description));
        Severity = DomainRules.Text(severity, nameof(severity));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ProjectId { get; }
    public string IssueCode { get; }
    public string Description { get; }
    public string Severity { get; }
    public string Status { get; }
}
