namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentContentBlob. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentContentBlob : OrganizationEntity
{
    public DocumentContentBlob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        string storageKey,
        string contentType,
        long sizeBytes,
        string sha256)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        StorageKey = DomainRules.Text(storageKey, nameof(storageKey));
        ContentType = DomainRules.Text(contentType, nameof(contentType));
        SizeBytes = DomainRules.NonnegativeLong(sizeBytes, nameof(sizeBytes));
        Sha256 = DomainRules.Hash(sha256, nameof(sha256));
    }

    public Guid StoredDocumentId { get; }
    public string StorageKey { get; }
    public string ContentType { get; }
    public long SizeBytes { get; }
    public string Sha256 { get; }
}
