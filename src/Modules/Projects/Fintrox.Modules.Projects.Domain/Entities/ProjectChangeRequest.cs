namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectChangeRequest; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectChangeRequest : OrganizationEntity
{
    public ProjectChangeRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid requestedByUserId,
        DateOnly requestedOn,
        string description,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        Description = DomainRules.Text(description, nameof(description));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ProjectId { get; }
    public Guid RequestedByUserId { get; }
    public DateOnly RequestedOn { get; }
    public string Description { get; }
    public string Status { get; }
}
