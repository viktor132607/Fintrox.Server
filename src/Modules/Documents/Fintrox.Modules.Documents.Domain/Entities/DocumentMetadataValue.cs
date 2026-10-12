namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentMetadataValue. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentMetadataValue : OrganizationEntity
{
    public DocumentMetadataValue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        Guid metadataFieldId,
        string value,
        DateTimeOffset updatedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        MetadataFieldId = DomainRules.Id(metadataFieldId, nameof(metadataFieldId));
        Value = DomainRules.Text(value, nameof(value));
        UpdatedAtUtc = DomainRules.Instant(updatedAtUtc, nameof(updatedAtUtc));
    }

    public Guid StoredDocumentId { get; }
    public Guid MetadataFieldId { get; }
    public string Value { get; }
    public DateTimeOffset UpdatedAtUtc { get; }
}
