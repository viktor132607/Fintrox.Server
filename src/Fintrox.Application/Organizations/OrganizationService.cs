using Fintrox.Application.Authorization;
using Fintrox.Application.Common;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Organizations;
using Fintrox.Domain.Organizations;

namespace Fintrox.Application.Organizations;

public sealed class OrganizationService(
    IOrganizationRepository repository,
    IOrganizationMembershipRepository memberships,
    IOrganizationAccessService access,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IOrganizationService
{
    public async Task<IReadOnlyList<OrganizationResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var organizations = await repository.ListForUserAsync(userId, cancellationToken);

        return organizations.Select(Map).ToArray();
    }

    public async Task<OrganizationResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        if (!await access.HasPermissionAsync(
                userId,
                id,
                Permissions.OrganizationsRead,
                cancellationToken))
        {
            return null;
        }

        var organization = await repository.GetAsync(id, false, cancellationToken);
        return organization is null ? null : Map(organization);
    }

    public async Task<OrganizationResponse> CreateAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        if (await repository.SlugExistsAsync(normalizedSlug, cancellationToken))
        {
            throw new OrganizationConflictException(
                $"An organization with slug '{normalizedSlug}' already exists.");
        }

        var now = timeProvider.GetUtcNow();

        var organization = Organization.Create(
            request.Name,
            request.LegalName,
            normalizedSlug,
            request.CountryCode,
            request.BaseCurrencyCode,
            request.TimeZoneId,
            request.RegistrationNumber,
            request.VatNumber,
            now);

        var ownerMembership = OrganizationMembership.Create(
            organization.Id,
            userId,
            OrganizationRole.Owner,
            now);

        await repository.AddAsync(organization, cancellationToken);
        await memberships.AddAsync(ownerMembership, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(organization);
    }

    public async Task<OrganizationResponse?> UpdateAsync(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureManagePermissionAsync(id, cancellationToken);

        var organization = await repository.GetAsync(id, true, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        organization.Update(
            request.Name,
            request.LegalName,
            request.CountryCode,
            request.BaseCurrencyCode,
            request.TimeZoneId,
            request.RegistrationNumber,
            request.VatNumber,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return Map(organization);
    }

    public async Task<bool> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await EnsureManagePermissionAsync(id, cancellationToken);

        var organization = await repository.GetAsync(id, true, cancellationToken);

        if (organization is null)
        {
            return false;
        }

        organization.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OrganizationResponse?> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await EnsureManagePermissionAsync(id, cancellationToken);

        var organization = await repository.GetAsync(id, true, cancellationToken);

        if (organization is null)
        {
            return null;
        }

        organization.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return Map(organization);
    }

    private async Task EnsureManagePermissionAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();

        if (!await access.HasPermissionAsync(
                userId,
                organizationId,
                Permissions.OrganizationsManage,
                cancellationToken))
        {
            throw new ForbiddenOperationException(
                "The current user cannot manage this organization.");
        }
    }

    private static OrganizationResponse Map(Organization organization)
    {
        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.LegalName,
            organization.Slug,
            organization.CountryCode,
            organization.BaseCurrencyCode,
            organization.TimeZoneId,
            organization.RegistrationNumber,
            organization.VatNumber,
            organization.IsActive,
            organization.CreatedAtUtc,
            organization.UpdatedAtUtc);
    }
}
