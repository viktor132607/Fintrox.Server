namespace Fintrox.Modules.Documents.Domain.Entities;

public sealed class DocumentTemplate : OrganizationEntity
{
    public DocumentTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        string contentType,
        string storageKey)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        ContentType = DomainRules.Text(contentType, nameof(contentType));
        StorageKey = DomainRules.Text(storageKey, nameof(storageKey));
    }

    public string Name { get; }
    public string ContentType { get; }
    public string StorageKey { get; }
}
