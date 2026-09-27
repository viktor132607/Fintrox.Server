using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fintrox.Application.Authorization;
using Fintrox.Application.Common.Interfaces;

namespace Fintrox.Api.Authentication;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;

            if (string.Equals(
                    principal?.FindFirstValue(
                        IntegrationClaims.ActorType),
                    IntegrationClaims.IntegrationActor,
                    StringComparison.Ordinal))
            {
                return null;
            }

            var value = principal?.FindFirstValue(
                    JwtRegisteredClaimNames.Sub)
                ?? principal?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }
    }

    public Guid RequireUserId()
    {
        return UserId
            ?? throw new InvalidOperationException(
                "An authenticated user is required for this operation.");
    }
}
