namespace Fintrox.Modules.Documents.Domain.Entities;

public sealed class DocumentVersion : OrganizationEntity
{
    public DocumentVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid documentId,
        int versionNumber,
        string storageKey,
        string sha256)
        : base(id, organizationId, createdAtUtc)
    {
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        StorageKey = DomainRules.Text(storageKey, nameof(storageKey));
        Sha256 = DomainRules.Hash(sha256, nameof(sha256));
    }

    public Guid DocumentId { get; }
    public int VersionNumber { get; }
    public string StorageKey { get; }
    public string Sha256 { get; }
}
