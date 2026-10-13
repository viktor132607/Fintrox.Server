using Fintrox.Application.Authorization;
using Fintrox.Application.Organizations;
using Fintrox.Domain.Organizations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Identity;

[TestClass]
public sealed class OrganizationAccessActivityTests
{
    [TestMethod]
    public async Task ActiveTenantAllowsOnlyPermissionsHeldByCurrentRole()
    {
        var state = new State(OrganizationRole.Accountant);
        Assert.IsTrue(await state.Access(Permissions.AccountingPost));
        Assert.IsTrue(await state.Access(Permissions.PaymentsWrite));
        Assert.IsFalse(await state.Access(Permissions.MembersManage));
        Assert.IsFalse(await state.Access(Permissions.OrganizationsReactivate));
    }

    [TestMethod]
    public async Task InactiveTenantDeniesEveryBusinessPermissionForActiveOwner()
    {
        var state = new State(OrganizationRole.Owner);
        state.Organization.Deactivate(DateTimeOffset.UtcNow);

        foreach (var permission in Permissions.All.Where(p =>
            p != Permissions.OrganizationsReactivate))
        {
            Assert.IsFalse(await state.Access(permission), permission);
        }

        Assert.IsTrue(await state.Access(Permissions.OrganizationsReactivate));
    }

    [TestMethod]
    public async Task InactiveTenantCannotBeReactivatedByAnAdministrator()
    {
        var state = new State(OrganizationRole.Administrator);
        state.Organization.Deactivate(DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Access(Permissions.OrganizationsReactivate));
        Assert.IsFalse(await state.Access(Permissions.OrganizationsManage));
        Assert.IsFalse(await state.Access(Permissions.AccountingRead));
    }

    [TestMethod]
    public async Task ReactivationRestoresExistingPermissionPolicy()
    {
        var state = new State(OrganizationRole.Owner);
        state.Organization.Deactivate(DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Access(Permissions.PaymentsWrite));
        Assert.IsTrue(await state.Access(Permissions.OrganizationsReactivate));
        state.Organization.Activate(DateTimeOffset.UtcNow);
        Assert.IsTrue(await state.Access(Permissions.PaymentsWrite));
    }

    [TestMethod]
    public async Task InactiveMembershipDeniesAccessEvenForOwner()
    {
        var state = new State(OrganizationRole.Owner);
        state.Membership.Deactivate(DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Access(Permissions.AccountingPost));
        Assert.IsFalse(await state.Access(Permissions.OrganizationsReactivate));
    }

    [TestMethod]
    public async Task MissingTenantOrMembershipDeniesAllIncludingReactivation()
    {
        var state = new State(OrganizationRole.Owner) { OrganizationExists = false };
        Assert.IsFalse(await state.Access(Permissions.AccountingRead));
        Assert.IsFalse(await state.Access(Permissions.OrganizationsReactivate));
        state.OrganizationExists = true;
        state.MembershipExists = false;
        Assert.IsFalse(await state.Access(Permissions.PaymentsWrite));
        Assert.IsFalse(await state.Access(Permissions.OrganizationsReactivate));
    }

    [TestMethod]
    public async Task WrongTenantOrEmptyIdentifiersFailClosed()
    {
        var state = new State(OrganizationRole.Owner);
        Assert.IsFalse(await state.Service.HasPermissionAsync(
            state.UserId, Guid.NewGuid(), Permissions.AccountingPost, CancellationToken.None));
        Assert.IsFalse(await state.Service.HasPermissionAsync(
            Guid.Empty, state.Organization.Id, Permissions.AccountingPost, CancellationToken.None));
        Assert.IsFalse(await state.Service.HasPermissionAsync(
            state.UserId, Guid.Empty, Permissions.AccountingPost, CancellationToken.None));
        Assert.IsFalse(await state.Service.HasPermissionAsync(
            state.UserId, state.Organization.Id, "unknown", CancellationToken.None));
    }

    private sealed class State : IOrganizationMembershipRepository, IOrganizationRepository
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Organization Organization { get; } = Organization.Create(
            "Example", null, "example", "BG", "EUR", "Europe/Sofia",
            null, null, DateTimeOffset.UtcNow);
        public OrganizationMembership Membership { get; }
        public OrganizationAccessService Service { get; }
        public bool MembershipExists { get; set; } = true;
        public bool OrganizationExists { get; set; } = true;

        public State(OrganizationRole role)
        {
            Membership = OrganizationMembership.Create(
                Organization.Id, UserId, role, DateTimeOffset.UtcNow);
            Service = new OrganizationAccessService(this, this);
        }

        public Task<bool> Access(string permission) => Service.HasPermissionAsync(
            UserId, Organization.Id, permission, CancellationToken.None);

        public Task<OrganizationMembership?> GetAsync(
            Guid organizationId, Guid userId, bool trackChanges,
            CancellationToken cancellationToken) => Task.FromResult(
                MembershipExists && Membership.OrganizationId == organizationId &&
                Membership.UserId == userId ? Membership : null);

        public Task<Organization?> GetAsync(
            Guid id, bool trackChanges, CancellationToken cancellationToken) =>
            Task.FromResult(OrganizationExists && Organization.Id == id ? Organization : null);

        public Task<IReadOnlyList<OrganizationMembership>> ListForUserAsync(
            Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<OrganizationMembership>> ListForOrganizationAsync(
            Guid organizationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> CountActiveOwnersAsync(
            Guid organizationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task LockOrganizationForMembershipChangeAsync(
            Guid organizationId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(
            OrganizationMembership membership, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        Task<IReadOnlyList<Organization>> IOrganizationRepository.ListForUserAsync(
            Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> SlugExistsAsync(
            string slug, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(Organization organization, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
