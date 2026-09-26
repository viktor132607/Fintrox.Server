using Fintrox.Contracts.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationRequestService
{
    Task<IReadOnlyList<IntegrationRequestResponse>> ListAsync(
        string? sourceSystem,
        string? externalId,
        string? eventType,
        CancellationToken cancellationToken);

    Task<IntegrationRequestResponse?> GetAsync(
        Guid requestId,
        CancellationToken cancellationToken);
}
