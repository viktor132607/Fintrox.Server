using Fintrox.Application.Integrations;
using Fintrox.Domain.Integrations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Integrations;

public sealed class IntegrationRequestRepository(
    FintroxDbContext dbContext) : IIntegrationRequestRepository
{
    public async Task<IReadOnlyList<IntegrationRequestRecord>> ListAsync(
        Guid organizationId,
        string? sourceSystem,
        string? externalId,
        string? eventType,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationRequestRecord> query = dbContext
            .IntegrationRequests
            .AsNoTracking()
            .Where(request =>
                request.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(sourceSystem))
        {
            query = query.Where(request =>
                request.SourceSystem == sourceSystem);
        }

        if (!string.IsNullOrWhiteSpace(externalId))
        {
            query = query.Where(request =>
                request.ExternalId == externalId);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(request =>
                request.EventType == eventType);
        }

        return await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenByDescending(request => request.Id)
            .Take(500)
            .ToArrayAsync(cancellationToken);
    }

    public Task<IntegrationRequestRecord?> GetAsync(
        Guid organizationId,
        Guid requestId,
        CancellationToken cancellationToken) =>
        dbContext.IntegrationRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(
                request =>
                    request.OrganizationId == organizationId &&
                    request.Id == requestId,
                cancellationToken);
}
