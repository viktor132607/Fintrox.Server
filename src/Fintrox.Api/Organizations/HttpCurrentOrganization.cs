using System.Security.Claims;
using Fintrox.Application.Authorization;
using Fintrox.Application.Common.Interfaces;

namespace Fintrox.Api.Organizations;

public sealed class HttpCurrentOrganization(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentOrganization
{
    public const string HeaderName = "X-Organization-Id";

    public Guid? OrganizationId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            var principal = httpContext?.User;

            if (string.Equals(
                    principal?.FindFirstValue(
                        IntegrationClaims.ActorType),
                    IntegrationClaims.IntegrationActor,
                    StringComparison.Ordinal))
            {
                var tokenValue = principal?.FindFirstValue(
                    IntegrationClaims.OrganizationId);

                if (!Guid.TryParse(
                        tokenValue,
                        out var tokenOrganizationId))
                {
                    return null;
                }

                var headerValue = httpContext?
                    .Request
                    .Headers[HeaderName]
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(headerValue))
                {
                    return tokenOrganizationId;
                }

                return Guid.TryParse(
                           headerValue,
                           out var headerOrganizationId) &&
                       headerOrganizationId == tokenOrganizationId
                    ? tokenOrganizationId
                    : null;
            }

            var value = httpContext?
                .Request
                .Headers[HeaderName]
                .FirstOrDefault();

            return Guid.TryParse(
                value,
                out var organizationId)
                ? organizationId
                : null;
        }
    }

    public Guid RequireOrganizationId()
    {
        return OrganizationId
            ?? throw new InvalidOperationException(
                $"A valid '{HeaderName}' header or integration organization claim is required for this operation.");
    }
}
