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

    Task AddAsync(
        OrganizationMembership membership,
        CancellationToken cancellationToken);
}
