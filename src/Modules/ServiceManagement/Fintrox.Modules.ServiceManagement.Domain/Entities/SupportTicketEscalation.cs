namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketEscalation domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketEscalation : OrganizationEntity
{
    public SupportTicketEscalation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid escalatedToUserId,
        string escalationReason,
        DateTimeOffset escalatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        EscalatedToUserId = DomainRules.Id(escalatedToUserId, nameof(escalatedToUserId));
        EscalationReason = DomainRules.Text(escalationReason, nameof(escalationReason));
        EscalatedAtUtc = DomainRules.Instant(escalatedAtUtc, nameof(escalatedAtUtc));
    }

    public Guid SupportTicketId { get; }
    public Guid EscalatedToUserId { get; }
    public string EscalationReason { get; }
    public DateTimeOffset EscalatedAtUtc { get; }
}
