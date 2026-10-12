namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmInteraction record. Domain invariants enforced at construction.</summary>
public sealed class CrmInteraction : OrganizationEntity
{
    public CrmInteraction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid contactId,
        string channel,
        string subject,
        DateTimeOffset occurredAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        ContactId = DomainRules.Id(contactId, nameof(contactId));
        Channel = DomainRules.Text(channel, nameof(channel));
        Subject = DomainRules.Text(subject, nameof(subject));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
    }

    public Guid CounterpartyId { get; }
    public Guid ContactId { get; }
    public string Channel { get; }
    public string Subject { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}
