namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentAccessEvent. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentAccessEvent : OrganizationEntity
{
    public DocumentAccessEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        Guid actorId,
        string accessType,
        DateTimeOffset accessedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        ActorId = DomainRules.Id(actorId, nameof(actorId));
        AccessType = DomainRules.Text(accessType, nameof(accessType));
        AccessedAtUtc = DomainRules.Instant(accessedAtUtc, nameof(accessedAtUtc));
    }

    public Guid StoredDocumentId { get; }
    public Guid ActorId { get; }
    public string AccessType { get; }
    public DateTimeOffset AccessedAtUtc { get; }
}
