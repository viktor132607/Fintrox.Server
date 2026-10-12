namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationReplayRequest domain record; references checked in application layer.</summary>
public sealed class IntegrationReplayRequest : OrganizationEntity
{
    public IntegrationReplayRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid externalEventId,
        Guid requestedByUserId,
        DateTimeOffset requestedAtUtc,
        string replayReason,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ExternalEventId = DomainRules.Id(externalEventId, nameof(externalEventId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        ReplayReason = DomainRules.Text(replayReason, nameof(replayReason));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ExternalEventId { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string ReplayReason { get; }
    public string Status { get; }
}
