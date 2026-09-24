using Fintrox.Application.Common.Interfaces;

namespace Fintrox.Api.Organizations;

public sealed class HttpCurrentOrganization(IHttpContextAccessor httpContextAccessor)
    : ICurrentOrganization
{
    public const string HeaderName = "X-Organization-Id";

    public Guid? OrganizationId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?
                .Request
                .Headers[HeaderName]
                .FirstOrDefault();

            return Guid.TryParse(value, out var organizationId)
                ? organizationId
                : null;
        }
    }

    public Guid RequireOrganizationId()
    {
        return OrganizationId
            ?? throw new InvalidOperationException(
                $"A valid '{HeaderName}' header is required for this operation.");
    }
}
