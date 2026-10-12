namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketAttachment domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketAttachment : OrganizationEntity
{
    public SupportTicketAttachment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid documentId,
        Guid attachedByUserId,
        DateTimeOffset attachedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        AttachedByUserId = DomainRules.Id(attachedByUserId, nameof(attachedByUserId));
        AttachedAtUtc = DomainRules.Instant(attachedAtUtc, nameof(attachedAtUtc));
    }

    public Guid SupportTicketId { get; }
    public Guid DocumentId { get; }
    public Guid AttachedByUserId { get; }
    public DateTimeOffset AttachedAtUtc { get; }
}
