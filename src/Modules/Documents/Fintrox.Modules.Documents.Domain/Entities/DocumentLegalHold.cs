namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentLegalHold. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentLegalHold : OrganizationEntity
{
    public DocumentLegalHold(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        string caseReference,
        string reason,
        DateTimeOffset placedAtUtc,
        bool released)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        CaseReference = DomainRules.Text(caseReference, nameof(caseReference));
        Reason = DomainRules.Text(reason, nameof(reason));
        PlacedAtUtc = DomainRules.Instant(placedAtUtc, nameof(placedAtUtc));
        Released = released;
    }

    public Guid StoredDocumentId { get; }
    public string CaseReference { get; }
    public string Reason { get; }
    public DateTimeOffset PlacedAtUtc { get; }
    public bool Released { get; }
}
