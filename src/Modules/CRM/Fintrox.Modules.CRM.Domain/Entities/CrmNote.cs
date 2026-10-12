namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmNote record. Domain invariants enforced at construction.</summary>
public sealed class CrmNote : OrganizationEntity
{
    public CrmNote(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid relatedRecordId,
        string relatedRecordType,
        Guid authorUserId,
        string body,
        DateTimeOffset writtenAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        RelatedRecordId = DomainRules.Id(relatedRecordId, nameof(relatedRecordId));
        RelatedRecordType = DomainRules.Text(relatedRecordType, nameof(relatedRecordType));
        AuthorUserId = DomainRules.Id(authorUserId, nameof(authorUserId));
        Body = DomainRules.Text(body, nameof(body));
        WrittenAtUtc = DomainRules.Instant(writtenAtUtc, nameof(writtenAtUtc));
    }

    public Guid RelatedRecordId { get; }
    public string RelatedRecordType { get; }
    public Guid AuthorUserId { get; }
    public string Body { get; }
    public DateTimeOffset WrittenAtUtc { get; }
}
