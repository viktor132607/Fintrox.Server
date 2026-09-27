using Fintrox.Contracts.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationClientService
{
    Task<IReadOnlyList<IntegrationClientResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IntegrationClientResponse?> GetAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken);

    Task<IntegrationClientSecretResponse> CreateAsync(
        CreateIntegrationClientRequest request,
        CancellationToken cancellationToken);

    Task<IntegrationClientResponse?> UpdateAsync(
        Guid integrationClientId,
        UpdateIntegrationClientRequest request,
        CancellationToken cancellationToken);

    Task<IntegrationClientSecretResponse?> RotateSecretAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken);

    Task<IntegrationClientResponse?> ActivateAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken);

    Task<IntegrationTokenResponse> ExchangeTokenAsync(
        IntegrationTokenRequest request,
        CancellationToken cancellationToken);
}
