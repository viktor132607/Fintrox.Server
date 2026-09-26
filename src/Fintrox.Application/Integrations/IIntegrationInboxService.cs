using Fintrox.Contracts.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationInboxService
{
    Task<IntegrationInboxResponse> ReceiveAsync(
        IntegrationInboxCommand command,
        CancellationToken cancellationToken);

    Task<IntegrationInboxResponse?> RetryAsync(
        Guid inboxId,
        CancellationToken cancellationToken);

    Task<IntegrationInboxResponse?> GetAsync(
        Guid inboxId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<IntegrationInboxResponse>> ListAsync(
        string? status,
        CancellationToken cancellationToken);
}
