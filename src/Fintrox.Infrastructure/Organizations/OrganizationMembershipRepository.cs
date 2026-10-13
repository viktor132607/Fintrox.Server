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
            .Where(membership =>
                membership.UserId == userId &&
                membership.IsActive)
            .OrderByDescending(membership => membership.Role)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationMembership>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrganizationMemberships
            .AsNoTracking()
            .Where(membership => membership.OrganizationId == organizationId)
            .OrderByDescending(membership => membership.IsActive)
            .ThenByDescending(membership => membership.Role)
            .ThenBy(membership => membership.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task LockOrganizationForMembershipChangeAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Organization membership changes require an active transaction.");
        }

        // PostgreSQL row lock serializes all membership mutations across
        // instances/processes. No LINQ composition around FOR UPDATE.
        // This table is the already-migrated legacy core organizations table.
        var locked = await dbContext.Organizations
            .FromSqlInterpolated(
                $"SELECT * FROM core.organizations WHERE id = {organizationId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (locked.Count != 1)
        {
            throw new OrganizationMembershipConflictException(
                "Organization does not exist.");
        }
    }

    public Task<int> CountActiveOwnersAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        return dbContext.OrganizationMemberships.CountAsync(
            membership =>
                membership.OrganizationId == organizationId &&
                membership.IsActive &&
                membership.Role == OrganizationRole.Owner,
            cancellationToken);
    }

    public async Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken)
    {
        await dbContext.OrganizationMemberships.AddAsync(
            membership,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
