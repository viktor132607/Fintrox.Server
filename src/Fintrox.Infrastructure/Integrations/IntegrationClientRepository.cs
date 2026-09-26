using Fintrox.Application.Integrations;
using Fintrox.Domain.Integrations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Integrations;

public sealed class IntegrationClientRepository(
    FintroxDbContext dbContext) : IIntegrationClientRepository
{
    public async Task<IReadOnlyList<IntegrationClient>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationClient> query = dbContext.IntegrationClients
            .AsNoTracking()
            .Where(client => client.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(client => client.IsActive);
        }

        return await query
            .OrderBy(client => client.Name)
            .ThenBy(client => client.ClientId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IntegrationClient?> GetAsync(
        Guid organizationId,
        Guid integrationClientId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationClient> query = dbContext.IntegrationClients;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            client =>
                client.OrganizationId == organizationId &&
                client.Id == integrationClientId,
            cancellationToken);
    }

    public async Task<IntegrationClient?> GetByClientIdAsync(
        string clientId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationClient> query = dbContext.IntegrationClients;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            client => client.ClientId == clientId,
            cancellationToken);
    }

    public Task<bool> ClientIdExistsAsync(
        string clientId,
        CancellationToken cancellationToken) =>
        dbContext.IntegrationClients.AnyAsync(
            client => client.ClientId == clientId,
            cancellationToken);

    public async Task AddAsync(
        IntegrationClient client,
        CancellationToken cancellationToken) =>
        await dbContext.IntegrationClients.AddAsync(
            client,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
