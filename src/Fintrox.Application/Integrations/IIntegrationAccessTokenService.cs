using Fintrox.Application.Identity;

namespace Fintrox.Application.Integrations;

public interface IIntegrationAccessTokenService
{
    AccessToken Create(
        Guid integrationClientId,
        string clientId,
        string clientName,
        Guid organizationId,
        IReadOnlyCollection<string> scopes);
}
