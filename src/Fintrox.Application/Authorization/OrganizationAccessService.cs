using Fintrox.Application.Organizations;

namespace Fintrox.Application.Authorization;

public sealed class OrganizationAccessService(
    IOrganizationMembershipRepository memberships) : IOrganizationAccessService
{
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid organizationId,
        string permission,
        CancellationToken cancellationToken)
    {
        var membership = await memberships.GetAsync(
            organizationId,
            userId,
            trackChanges: false,
            cancellationToken);

        return membership is { IsActive: true } &&
               RolePermissions.Has(membership.Role, permission);
    }
}
