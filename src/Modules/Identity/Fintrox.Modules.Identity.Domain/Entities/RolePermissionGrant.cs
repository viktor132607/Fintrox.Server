namespace Fintrox.Modules.Identity.Domain.Entities;

public sealed class RolePermissionGrant : OrganizationEntity
{
    public RolePermissionGrant(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid roleId,
        Guid permissionId)
        : base(id, organizationId, createdAtUtc)
    {
        RoleId = DomainRules.Id(roleId, nameof(roleId));
        PermissionId = DomainRules.Id(permissionId, nameof(permissionId));
    }

    public Guid RoleId { get; }
    public Guid PermissionId { get; }
}
