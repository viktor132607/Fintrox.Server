using Fintrox.Contracts.Organizations;

namespace Fintrox.Application.Organizations;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationResponse>> ListAsync(CancellationToken cancellationToken);

    Task<OrganizationResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<OrganizationResponse> CreateAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken);

    Task<OrganizationResponse?> UpdateAsync(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken);

    Task<OrganizationResponse?> ActivateAsync(Guid id, CancellationToken cancellationToken);
}
