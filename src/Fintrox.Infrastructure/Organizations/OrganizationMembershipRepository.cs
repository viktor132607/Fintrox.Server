using Fintrox.Application.Organizations;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Organizations;

public sealed class OrganizationMembershipRepository(
    FintroxDbContext dbContext) : IOrganizationMembershipRepository
{
    public async Task<OrganizationMembership?> GetAsync(
        Guid organizationId,
        Guid userId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<OrganizationMembership> query = dbContext.OrganizationMemberships;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            membership =>
                membership.OrganizationId == organizationId &&
                membership.UserId == userId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationMembership>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(membership => membership.UserId == userId && membership.IsActive)
            .OrderByDescending(membership => membership.Role)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken)
    {
        await dbContext.OrganizationMemberships.AddAsync(
            membership,
            cancellationToken);
    }
}
