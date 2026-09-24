using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Organizations;

public interface IOrganizationRepository
{
    Task<IReadOnlyList<Organization>> ListAsync(CancellationToken cancellationToken);

    Task<Organization?> GetAsync(Guid id, bool trackChanges, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    Task AddAsync(Organization organization, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
