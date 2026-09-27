using Fintrox.Application.Authorization;
using Fintrox.Application.Common;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Identity;
using Fintrox.Contracts.Organizations;
using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Organizations;

public sealed class OrganizationMemberService(
    IOrganizationMembershipRepository memberships,
    IOrganizationAccessService access,
    IUserDirectory users,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IOrganizationMemberService
{
    public async Task<IReadOnlyList<OrganizationMemberResponse>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        await EnsureManagerAsync(organizationId, cancellationToken);

        var organizationMemberships = await memberships.ListForOrganizationAsync(
            organizationId,
            cancellationToken);

        var userIds = organizationMemberships.Select(x => x.UserId).ToArray();
        var directory = await users.ListByIdsAsync(userIds, cancellationToken);

        return organizationMemberships
            .Where(x => directory.ContainsKey(x.UserId))
            .Select(x => Map(x, directory[x.UserId]))
            .ToArray();
    }

    public async Task<OrganizationMemberResponse> AddAsync(
        Guid organizationId,
        AddOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        var manager = await EnsureManagerAsync(organizationId, cancellationToken);
        var targetRole = ParseRole(request.Role);
        EnsureCanAssign(manager.Role, targetRole);

        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is null || !user.IsActive)
        {
            throw new OrganizationMembershipConflictException(
                "No active Fintrox user exists with this email.");
        }

        var existing = await memberships.GetAsync(
            organizationId,
            user.Id,
            trackChanges: true,
            cancellationToken);

        var now = timeProvider.GetUtcNow();

        if (existing is { IsActive: true })
        {
            throw new OrganizationMembershipConflictException(
                "The user is already an active member of this organization.");
        }

        OrganizationMembership membership;

        if (existing is null)
        {
            membership = OrganizationMembership.Create(
                organizationId,
                user.Id,
                targetRole,
                now);

            await memberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            existing.ChangeRole(targetRole, now);
            existing.Activate(now);
            membership = existing;
        }

        await memberships.SaveChangesAsync(cancellationToken);
        return Map(membership, user);
    }

    public async Task<OrganizationMemberResponse?> ChangeRoleAsync(
        Guid organizationId,
        Guid userId,
        UpdateOrganizationMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var manager = await EnsureManagerAsync(organizationId, cancellationToken);
        var target = await memberships.GetAsync(
            organizationId,
            userId,
            trackChanges: true,
            cancellationToken);

        if (target is null)
        {
            return null;
        }

        EnsureCanManageMember(manager, target);

        var targetRole = ParseRole(request.Role);
        EnsureCanAssign(manager.Role, targetRole);

        if (target.IsActive &&
            target.Role == OrganizationRole.Owner &&
            targetRole != OrganizationRole.Owner)
        {
            await EnsureNotLastOwnerAsync(organizationId, cancellationToken);
        }

        target.ChangeRole(targetRole, timeProvider.GetUtcNow());
        await memberships.SaveChangesAsync(cancellationToken);

        var directory = await users.ListByIdsAsync([target.UserId], cancellationToken);
        return directory.TryGetValue(target.UserId, out var user)
            ? Map(target, user)
            : null;
    }

    public async Task<bool> DeactivateAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var manager = await EnsureManagerAsync(organizationId, cancellationToken);
        var target = await memberships.GetAsync(
            organizationId,
            userId,
            trackChanges: true,
            cancellationToken);

        if (target is null)
        {
            return false;
        }

        EnsureCanManageMember(manager, target);

        if (target.IsActive && target.Role == OrganizationRole.Owner)
        {
            await EnsureNotLastOwnerAsync(organizationId, cancellationToken);
        }

        target.Deactivate(timeProvider.GetUtcNow());
        await memberships.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<OrganizationMembership> EnsureManagerAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        if (!await access.HasPermissionAsync(
                userId,
                organizationId,
                Permissions.MembersManage,
                cancellationToken))
        {
            throw new ForbiddenOperationException(
                "The current user cannot manage organization members.");
        }

        return await memberships.GetAsync(
                   organizationId,
                   userId,
                   trackChanges: false,
                   cancellationToken)
               ?? throw new ForbiddenOperationException(
                   "The current user is not an organization member.");
    }

    private static void EnsureCanManageMember(
        OrganizationMembership manager,
        OrganizationMembership target)
    {
        if (manager.Role == OrganizationRole.Owner)
        {
            return;
        }

        if (target.Role >= manager.Role)
        {
            throw new ForbiddenOperationException(
                "A non-owner cannot manage a member with an equal or higher role.");
        }
    }

    private static void EnsureCanAssign(
        OrganizationRole managerRole,
        OrganizationRole targetRole)
    {
        if (managerRole == OrganizationRole.Owner)
        {
            return;
        }

        if (targetRole >= managerRole)
        {
            throw new ForbiddenOperationException(
                "A non-owner cannot assign an equal or higher role.");
        }
    }

    private async Task EnsureNotLastOwnerAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (await memberships.CountActiveOwnersAsync(
                organizationId,
                cancellationToken) <= 1)
        {
            throw new OrganizationMembershipConflictException(
                "The last active owner cannot be demoted or deactivated.");
        }
    }

    private static OrganizationRole ParseRole(string value)
    {
        if (!Enum.TryParse<OrganizationRole>(
                value,
                ignoreCase: true,
                out var role) ||
            !Enum.IsDefined(role))
        {
            throw new OrganizationMembershipConflictException(
                "Unknown organization role.");
        }

        return role;
    }

    private static OrganizationMemberResponse Map(
        OrganizationMembership membership,
        UserDirectoryEntry user)
    {
        return new OrganizationMemberResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            membership.Role.ToString(),
            membership.IsActive,
            membership.CreatedAtUtc,
            membership.UpdatedAtUtc);
    }
}
