using Fintrox.Application.Authorization;
using Fintrox.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Fintrox.Api.Authorization;

public sealed class PermissionAuthorizationHandler(
    ICurrentUser currentUser,
    ICurrentOrganization currentOrganization,
    IOrganizationAccessService accessService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = currentUser.UserId;
        var organizationId = currentOrganization.OrganizationId;

        if (userId is null || organizationId is null)
        {
            return;
        }

        if (await accessService.HasPermissionAsync(
                userId.Value,
                organizationId.Value,
                requirement.Permission,
                CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }
}
