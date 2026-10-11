namespace Fintrox.Modules.Documents.Domain.Entities;

public sealed class DocumentLink : OrganizationEntity
{
    public DocumentLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid documentId,
        string resourceType,
        Guid resourceId)
        : base(id, organizationId, createdAtUtc)
    {
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
    }

    public Guid DocumentId { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }
}
