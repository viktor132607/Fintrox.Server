namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmAttachment record. Domain invariants enforced at construction.</summary>
public sealed class CrmAttachment : OrganizationEntity
{
    public CrmAttachment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid relatedRecordId,
        Guid documentId,
        string relatedRecordType,
        Guid attachedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        RelatedRecordId = DomainRules.Id(relatedRecordId, nameof(relatedRecordId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        RelatedRecordType = DomainRules.Text(relatedRecordType, nameof(relatedRecordType));
        AttachedByUserId = DomainRules.Id(attachedByUserId, nameof(attachedByUserId));
    }

    public Guid RelatedRecordId { get; }
    public Guid DocumentId { get; }
    public string RelatedRecordType { get; }
    public Guid AttachedByUserId { get; }
}
