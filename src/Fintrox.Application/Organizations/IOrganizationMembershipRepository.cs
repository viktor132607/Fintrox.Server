using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Organizations;

public interface IOrganizationMembershipRepository
{
    Task<OrganizationMembership?> GetAsync(
        Guid organizationId,
        Guid userId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationMembership>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationMembership>> ListForOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<int> CountActiveOwnersAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Acquires a PostgreSQL organization-row lock in the caller's transaction.
    /// Every member mutation must hold this lock until commit. This prevents
    /// concurrent last-owner demotions, removals and stale-manager writes.
    /// </summary>
    Task LockOrganizationForMembershipChangeAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
