namespace Fintrox.Modules.Documents.Domain.Entities;

public sealed class StoredDocument : OrganizationEntity
{
    public StoredDocument(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string contentType,
        string storageKey,
        long sizeBytes,
        string sha256)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        ContentType = DomainRules.Text(contentType, nameof(contentType));
        StorageKey = DomainRules.Text(storageKey, nameof(storageKey));
        SizeBytes = DomainRules.NonnegativeLong(sizeBytes, nameof(sizeBytes));
        Sha256 = DomainRules.Hash(sha256, nameof(sha256));
    }

    public string Name { get; }
    public string ContentType { get; }
    public string StorageKey { get; }
    public long SizeBytes { get; }
    public string Sha256 { get; }
}
