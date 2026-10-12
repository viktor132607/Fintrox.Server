namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyActivityTimeline. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyActivityTimeline : OrganizationEntity
{
    public CounterpartyActivityTimeline(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string eventType,
        Guid sourceReferenceId,
        DateTimeOffset occurredAtUtc,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        SourceReferenceId = DomainRules.Id(sourceReferenceId, nameof(sourceReferenceId));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid CounterpartyId { get; }
    public string EventType { get; }
    public Guid SourceReferenceId { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Description { get; }
}
