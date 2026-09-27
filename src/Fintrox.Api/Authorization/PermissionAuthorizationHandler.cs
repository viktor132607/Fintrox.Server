using System.Security.Claims;
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
        var organizationId = currentOrganization.OrganizationId;

        if (organizationId is null)
        {
            return;
        }

        if (IsIntegrationPrincipal(context.User))
        {
            if (HasIntegrationScope(
                    context.User,
                    organizationId.Value,
                    requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return;
        }

        var userId = currentUser.UserId;

        if (userId is null)
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

    private static bool IsIntegrationPrincipal(
        ClaimsPrincipal principal) =>
        string.Equals(
            principal.FindFirstValue(
                IntegrationClaims.ActorType),
            IntegrationClaims.IntegrationActor,
            StringComparison.Ordinal);

    private static bool HasIntegrationScope(
        ClaimsPrincipal principal,
        Guid organizationId,
        string requiredPermission)
    {
        var organizationClaim = principal.FindFirstValue(
            IntegrationClaims.OrganizationId);

        if (!Guid.TryParse(
                organizationClaim,
                out var tokenOrganizationId) ||
            tokenOrganizationId != organizationId)
        {
            return false;
        }

        return principal
            .FindAll(IntegrationClaims.Scope)
            .Any(claim => string.Equals(
                claim.Value,
                requiredPermission,
                StringComparison.Ordinal));
    }
}
