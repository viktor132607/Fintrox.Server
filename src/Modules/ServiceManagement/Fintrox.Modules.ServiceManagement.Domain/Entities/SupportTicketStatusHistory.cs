namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketStatusHistory domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketStatusHistory : OrganizationEntity
{
    public SupportTicketStatusHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        string oldStatus,
        string newStatus,
        DateTimeOffset changedAtUtc,
        Guid changedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        OldStatus = DomainRules.Text(oldStatus, nameof(oldStatus));
        NewStatus = DomainRules.Text(newStatus, nameof(newStatus));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
    }

    public Guid SupportTicketId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public Guid ChangedByUserId { get; }
}
