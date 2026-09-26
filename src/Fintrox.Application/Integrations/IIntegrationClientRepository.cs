using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationClientRepository
{
    Task<IReadOnlyList<IntegrationClient>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IntegrationClient?> GetAsync(
        Guid organizationId,
        Guid integrationClientId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IntegrationClient?> GetByClientIdAsync(
        string clientId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> ClientIdExistsAsync(
        string clientId,
        CancellationToken cancellationToken);

    Task AddAsync(
        IntegrationClient client,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
