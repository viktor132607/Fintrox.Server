namespace Fintrox.Application.Authorization;

public interface IOrganizationAccessService
{
    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid organizationId,
        string permission,
        CancellationToken cancellationToken);
}
