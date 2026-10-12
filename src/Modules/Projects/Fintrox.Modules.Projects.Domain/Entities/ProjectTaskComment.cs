namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectTaskComment; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectTaskComment : OrganizationEntity
{
    public ProjectTaskComment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectTaskId,
        Guid authorUserId,
        string content,
        DateTimeOffset postedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectTaskId = DomainRules.Id(projectTaskId, nameof(projectTaskId));
        AuthorUserId = DomainRules.Id(authorUserId, nameof(authorUserId));
        Content = DomainRules.Text(content, nameof(content));
        PostedAtUtc = DomainRules.Instant(postedAtUtc, nameof(postedAtUtc));
    }

    public Guid ProjectTaskId { get; }
    public Guid AuthorUserId { get; }
    public string Content { get; }
    public DateTimeOffset PostedAtUtc { get; }
}
