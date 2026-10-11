namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class CrmActivity : OrganizationEntity
{
    public CrmActivity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string subject,
        DateTimeOffset scheduledAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Subject = DomainRules.Text(subject, nameof(subject));
        ScheduledAtUtc = DomainRules.Instant(scheduledAtUtc, nameof(scheduledAtUtc));
    }

    public Guid CounterpartyId { get; }
    public string Subject { get; }
    public DateTimeOffset ScheduledAtUtc { get; }
}
