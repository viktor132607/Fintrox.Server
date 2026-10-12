namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplierCollaborationMessage domain record. No cross-module navigation properties.</summary>
public sealed class SupplierCollaborationMessage : OrganizationEntity
{
    public SupplierCollaborationMessage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        Guid relatedDocumentId,
        string subject,
        string body,
        DateTimeOffset sentAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        RelatedDocumentId = DomainRules.Id(relatedDocumentId, nameof(relatedDocumentId));
        Subject = DomainRules.Text(subject, nameof(subject));
        Body = DomainRules.Text(body, nameof(body));
        SentAtUtc = DomainRules.Instant(sentAtUtc, nameof(sentAtUtc));
    }

    public Guid SupplierId { get; }
    public Guid RelatedDocumentId { get; }
    public string Subject { get; }
    public string Body { get; }
    public DateTimeOffset SentAtUtc { get; }
}
