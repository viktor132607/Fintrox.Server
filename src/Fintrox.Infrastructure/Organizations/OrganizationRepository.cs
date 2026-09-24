using Fintrox.Application.Organizations;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Organizations;

public sealed class OrganizationRepository(FintroxDbContext dbContext) : IOrganizationRepository
{
    public async Task<IReadOnlyList<Organization>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await (
            from organization in dbContext.Organizations.AsNoTracking()
            join membership in dbContext.OrganizationMemberships.AsNoTracking()
                on organization.Id equals membership.OrganizationId
            where membership.UserId == userId &&
                  membership.IsActive &&
                  organization.IsActive
            orderby organization.Name
            select organization)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Organization?> GetAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Organization> query = dbContext.Organizations;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            organization => organization.Id == id,
            cancellationToken);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        return dbContext.Organizations.AnyAsync(
            organization => organization.Slug == slug,
            cancellationToken);
    }

    public async Task AddAsync(
        Organization organization,
        CancellationToken cancellationToken)
    {
        await dbContext.Organizations.AddAsync(organization, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
