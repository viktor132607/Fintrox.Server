using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationRequestRepository
{
    Task<IReadOnlyList<IntegrationRequestRecord>> ListAsync(
        Guid organizationId,
        string? sourceSystem,
        string? externalId,
        string? eventType,
        CancellationToken cancellationToken);

    Task<IntegrationRequestRecord?> GetAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken);
}
