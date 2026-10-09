using System.Security.Claims;
using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Application.Organizations;

namespace Fintrox.Application.Identity;

// Signature, issuer, audience and lifetime are checked by the bearer handler first.
// This check deliberately reads current state rather than caching revocable access.
public sealed class AccessTokenValidator(
    IUserDirectory users,
    IIntegrationClientRepository clients,
    IOrganizationRepository organizations) : IAccessTokenValidator
{
    public async Task<bool> IsActiveAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(SingleValue(principal, "sub"), out var subjectId) ||
            subjectId == Guid.Empty)
        {
            return false;
        }

        var actorClaims = principal.FindAll(IntegrationClaims.ActorType).ToArray();
        if (actorClaims.Length == 0)
        {
            // Integration-shaped tokens must never fall back to user authentication.
            if (principal.HasClaim(claim =>
                    claim.Type == IntegrationClaims.ClientId ||
                    claim.Type == IntegrationClaims.OrganizationId ||
                    claim.Type == IntegrationClaims.Scope))
            {
                return false;
            }

            var directory = await users.ListByIdsAsync([subjectId], cancellationToken);
            return directory.TryGetValue(subjectId, out var user) && user.IsActive;
        }

        if (actorClaims.Length != 1 ||
            actorClaims[0].Value != IntegrationClaims.IntegrationActor ||
            !Guid.TryParse(SingleValue(principal, IntegrationClaims.OrganizationId), out var organizationId) ||
            organizationId == Guid.Empty)
        {
            return false;
        }

        var clientId = SingleValue(principal, IntegrationClaims.ClientId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return false;
        }

        var client = await clients.GetAsync(
            organizationId, subjectId, trackChanges: false, cancellationToken);
        if (client is not { IsActive: true } ||
            client.Id != subjectId || client.OrganizationId != organizationId ||
            !string.Equals(client.ClientId, clientId, StringComparison.Ordinal))
        {
            return false;
        }

        var organization = await organizations.GetAsync(
            organizationId, trackChanges: false, cancellationToken);
        if (organization is not { IsActive: true })
        {
            return false;
        }

        var currentScopes = client.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
        var tokenScopes = principal.FindAll(IntegrationClaims.Scope).ToArray();
        // Adding a scope never elevates an existing token; removing one invalidates it.
        return tokenScopes.Length > 0 &&
               tokenScopes.All(scope => currentScopes.Contains(scope.Value));
    }

    private static string? SingleValue(ClaimsPrincipal principal, string type)
    {
        var claims = principal.FindAll(type).Take(2).ToArray();
        return claims.Length == 1 ? claims[0].Value : null;
    }
}
