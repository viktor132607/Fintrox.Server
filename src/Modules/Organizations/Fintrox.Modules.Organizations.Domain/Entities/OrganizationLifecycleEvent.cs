namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationLifecycleEvent record.</summary>
public sealed class OrganizationLifecycleEvent : OrganizationEntity
{
    public OrganizationLifecycleEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string eventCode,
        string reason,
        Guid actorUserId,
        DateTimeOffset occurredAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        Reason = DomainRules.Text(reason, nameof(reason));
        ActorUserId = DomainRules.Id(actorUserId, nameof(actorUserId));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
    }

    public string EventCode { get; }
    public string Reason { get; }
    public Guid ActorUserId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}
