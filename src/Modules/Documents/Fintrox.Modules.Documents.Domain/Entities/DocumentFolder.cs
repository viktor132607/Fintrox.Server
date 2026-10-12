namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentFolder. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentFolder : OrganizationEntity
{
    public DocumentFolder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid parentFolderId,
        Guid ownerUserId,
        string visibility)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        ParentFolderId = DomainRules.Id(parentFolderId, nameof(parentFolderId));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        Visibility = DomainRules.Text(visibility, nameof(visibility));
    }

    public string Name { get; }
    public Guid ParentFolderId { get; }
    public Guid OwnerUserId { get; }
    public string Visibility { get; }
}
