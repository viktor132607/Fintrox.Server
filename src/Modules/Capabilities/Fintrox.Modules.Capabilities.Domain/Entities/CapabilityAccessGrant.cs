namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityAccessGrant record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityAccessGrant : OrganizationEntity
{
    public CapabilityAccessGrant(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        Guid principalId,
        string permissionCode,
        Guid grantedByUserId,
        DateTimeOffset grantedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        PrincipalId = DomainRules.Id(principalId, nameof(principalId));
        PermissionCode = DomainRules.Text(permissionCode, nameof(permissionCode));
        GrantedByUserId = DomainRules.Id(grantedByUserId, nameof(grantedByUserId));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
    }

    public Guid CapabilityId { get; }
    public Guid PrincipalId { get; }
    public string PermissionCode { get; }
    public Guid GrantedByUserId { get; }
    public DateTimeOffset GrantedAtUtc { get; }
}
