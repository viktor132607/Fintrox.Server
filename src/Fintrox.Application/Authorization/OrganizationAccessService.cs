using Fintrox.Application.Organizations;
using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Authorization;

/// <summary>
/// Enforces current tenant activity and membership on every protected organization operation.
/// An inactive tenant can only be reactivated by an active owner.
/// </summary>
public sealed class OrganizationAccessService(
    IOrganizationMembershipRepository memberships,
    IOrganizationRepository organizations) : IOrganizationAccessService
{
    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid organizationId,
        string permission,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || organizationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var membership = await memberships.GetAsync(
            organizationId,
            userId,
            trackChanges: false,
            cancellationToken);

        if (membership is not { IsActive: true } ||
            !RolePermissions.Has(membership.Role, permission))
        {
            return false;
        }

        // Resolve current organization state rather than trusting a user membership or a JWT.
        var organization = await organizations.GetAsync(
            organizationId,
            trackChanges: false,
            cancellationToken);

        if (organization is null)
        {
            return false;
        }

        if (organization.IsActive)
        {
            return true;
        }

        // The exception does not grant read/write access to any financial or operational data.
        return permission == Permissions.OrganizationsReactivate &&
               membership.Role == OrganizationRole.Owner;
    }
}
