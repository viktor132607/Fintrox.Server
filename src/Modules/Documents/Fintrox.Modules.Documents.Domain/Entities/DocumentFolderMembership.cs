namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentFolderMembership. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentFolderMembership : OrganizationEntity
{
    public DocumentFolderMembership(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid folderId,
        Guid storedDocumentId,
        DateTimeOffset linkedAtUtc,
        Guid linkedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        FolderId = DomainRules.Id(folderId, nameof(folderId));
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        LinkedAtUtc = DomainRules.Instant(linkedAtUtc, nameof(linkedAtUtc));
        LinkedByUserId = DomainRules.Id(linkedByUserId, nameof(linkedByUserId));
    }

    public Guid FolderId { get; }
    public Guid StoredDocumentId { get; }
    public DateTimeOffset LinkedAtUtc { get; }
    public Guid LinkedByUserId { get; }
}
