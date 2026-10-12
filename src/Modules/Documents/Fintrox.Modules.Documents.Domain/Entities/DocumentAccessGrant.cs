namespace Fintrox.Modules.Documents.Domain.Entities;

/// <summary>Tenant-scoped DocumentAccessGrant. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class DocumentAccessGrant : OrganizationEntity
{
    public DocumentAccessGrant(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storedDocumentId,
        Guid principalId,
        string permissionCode,
        DateTimeOffset grantedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        StoredDocumentId = DomainRules.Id(storedDocumentId, nameof(storedDocumentId));
        PrincipalId = DomainRules.Id(principalId, nameof(principalId));
        PermissionCode = DomainRules.Text(permissionCode, nameof(permissionCode));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
    }

    public Guid StoredDocumentId { get; }
    public Guid PrincipalId { get; }
    public string PermissionCode { get; }
    public DateTimeOffset GrantedAtUtc { get; }
}
