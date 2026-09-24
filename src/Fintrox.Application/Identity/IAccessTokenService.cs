namespace Fintrox.Application.Identity;

public interface IAccessTokenService
{
    AccessToken Create(
        Guid userId,
        string email,
        string displayName);
}
