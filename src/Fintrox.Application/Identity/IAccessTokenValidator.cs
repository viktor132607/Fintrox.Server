using System.Security.Claims;

namespace Fintrox.Application.Identity;

public interface IAccessTokenValidator
{
    Task<bool> IsActiveAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken);
}
