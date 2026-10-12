namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketMessage domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketMessage : OrganizationEntity
{
    public SupportTicketMessage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid senderUserId,
        string body,
        DateTimeOffset sentAtUtc,
        bool isInternal)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        SenderUserId = DomainRules.Id(senderUserId, nameof(senderUserId));
        Body = DomainRules.Text(body, nameof(body));
        SentAtUtc = DomainRules.Instant(sentAtUtc, nameof(sentAtUtc));
        IsInternal = isInternal;
    }

    public Guid SupportTicketId { get; }
    public Guid SenderUserId { get; }
    public string Body { get; }
    public DateTimeOffset SentAtUtc { get; }
    public bool IsInternal { get; }
}
