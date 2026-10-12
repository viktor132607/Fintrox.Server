namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentIndexEntry. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentIndexEntry : OrganizationEntity
{
    public DocumentIndexEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        string indexName,
        DateTimeOffset indexedAtUtc,
        int indexVersion)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        IndexName = DomainRules.Text(indexName, nameof(indexName));
        IndexedAtUtc = DomainRules.Instant(indexedAtUtc, nameof(indexedAtUtc));
        IndexVersion = DomainRules.PositiveInt(indexVersion, nameof(indexVersion));
    }

    public Guid StoredDocumentId { get; }
    public string IndexName { get; }
    public DateTimeOffset IndexedAtUtc { get; }
    public int IndexVersion { get; }
}
