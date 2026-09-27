using Fintrox.Contracts.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationFailureService
{
    Task<IReadOnlyList<IntegrationFailureResponse>> ListAsync(
        bool includeResolved,
        CancellationToken cancellationToken);

    Task<IntegrationFailureResponse?> GetAsync(
        Guid failureId,
        CancellationToken cancellationToken);

    Task<IntegrationFailureResponse?> RetryAsync(
        Guid failureId,
        CancellationToken cancellationToken);
}
