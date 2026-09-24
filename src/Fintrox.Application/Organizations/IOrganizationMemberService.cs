using Fintrox.Contracts.Organizations;

namespace Fintrox.Application.Organizations;

public interface IOrganizationMemberService
{
    Task<IReadOnlyList<OrganizationMemberResponse>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<OrganizationMemberResponse> AddAsync(
        Guid organizationId,
        AddOrganizationMemberRequest request,
        CancellationToken cancellationToken);

    Task<OrganizationMemberResponse?> ChangeRoleAsync(
        Guid organizationId,
        Guid userId,
        UpdateOrganizationMemberRoleRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken);
}
