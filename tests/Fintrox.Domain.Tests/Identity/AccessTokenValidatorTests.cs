using System.Security.Claims;
using Fintrox.Application.Authorization;
using Fintrox.Application.Identity;
using Fintrox.Application.Integrations;
using Fintrox.Application.Organizations;
using Fintrox.Domain.Integrations;
using Fintrox.Domain.Organizations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Identity;

[TestClass]
public sealed class AccessTokenValidatorTests
{
    [TestMethod]
    public async Task ActiveUserIsAcceptedButDeactivationIsObservedOnNextRequest()
    {
        var state = new State();
        var principal = User(state.UserId);
        Assert.IsTrue(await state.Validator.IsActiveAsync(principal, CancellationToken.None));
        state.UserActive = false;
        Assert.IsFalse(await state.Validator.IsActiveAsync(principal, CancellationToken.None));
    }

    [TestMethod]
    public async Task MissingUserIsRejected()
    {
        var state = new State { UserExists = false };
        Assert.IsFalse(await state.Validator.IsActiveAsync(User(state.UserId), CancellationToken.None));
    }

    [TestMethod]
    public async Task MalformedMissingEmptyAndDuplicateSubjectsAreRejected()
    {
        var state = new State();
        foreach (var claims in new Claim[][]
        {
            [], [new("sub", "invalid")], [new("sub", Guid.Empty.ToString())],
            [new("sub", state.UserId.ToString()), new("sub", state.UserId.ToString())]
        })
        {
            Assert.IsFalse(await state.Validator.IsActiveAsync(Principal(claims), CancellationToken.None));
        }
    }

    [TestMethod]
    public async Task UnknownDuplicateOrMissingIntegrationActorCannotFallBackToUser()
    {
        var state = new State();
        foreach (var extra in new Claim[][]
        {
            [new(IntegrationClaims.ActorType, "unknown")],
            [new(IntegrationClaims.ActorType, "integration"), new(IntegrationClaims.ActorType, "integration")],
            [new(IntegrationClaims.ClientId, "fic_test")],
            [new(IntegrationClaims.OrganizationId, state.Organization.Id.ToString())],
            [new(IntegrationClaims.Scope, "sales.read")]
        })
        {
            Assert.IsFalse(await state.Validator.IsActiveAsync(
                Principal([new("sub", state.UserId.ToString()), .. extra]), CancellationToken.None));
        }
    }

    [TestMethod]
    public async Task ActiveIntegrationWithCurrentScopeIsAccepted()
    {
        var state = new State();
        Assert.IsTrue(await state.Validator.IsActiveAsync(state.Integration(), CancellationToken.None));
    }

    [TestMethod]
    public async Task ClientDeactivationImmediatelyRejectsPreviouslyAcceptedToken()
    {
        var state = new State();
        var token = state.Integration();
        Assert.IsTrue(await state.Validator.IsActiveAsync(token, CancellationToken.None));
        state.Client.Deactivate(DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Validator.IsActiveAsync(token, CancellationToken.None));
    }

    [TestMethod]
    public async Task MissingClientOrOrganizationIsRejected()
    {
        var state = new State { ClientExists = false };
        Assert.IsFalse(await state.Validator.IsActiveAsync(state.Integration(), CancellationToken.None));
        state.ClientExists = true;
        state.OrganizationExists = false;
        Assert.IsFalse(await state.Validator.IsActiveAsync(state.Integration(), CancellationToken.None));
    }

    [TestMethod]
    public async Task OrganizationDeactivationRejectsExistingIntegrationToken()
    {
        var state = new State();
        state.Organization.Deactivate(DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Validator.IsActiveAsync(state.Integration(), CancellationToken.None));
    }

    [TestMethod]
    public async Task RevokedScopeRejectsExistingTokenAndAdditionalScopesDoNotElevateIt()
    {
        var state = new State();
        var token = state.Integration();
        state.Client.Update("Test", "sales.read sales.write reports.read", DateTimeOffset.UtcNow);
        Assert.IsTrue(await state.Validator.IsActiveAsync(token, CancellationToken.None));
        Assert.AreEqual(1, token.FindAll(IntegrationClaims.Scope).Count());
        state.Client.Update("Test", "sales.write", DateTimeOffset.UtcNow);
        Assert.IsFalse(await state.Validator.IsActiveAsync(token, CancellationToken.None));
    }

    [TestMethod]
    public async Task TokenScopesMustBePresentExactAndNotEmpty()
    {
        var state = new State();
        foreach (var scope in new[] { "Sales.Read", "sales.read sales.write", "unknown", "" })
        {
            Assert.IsFalse(await state.Validator.IsActiveAsync(state.Integration(scope), CancellationToken.None));
        }
        var claims = state.Integration().Claims.Where(c => c.Type != IntegrationClaims.Scope);
        Assert.IsFalse(await state.Validator.IsActiveAsync(Principal(claims), CancellationToken.None));
    }

    [TestMethod]
    public async Task WrongTenantSubjectOrPublicClientIdIsRejected()
    {
        var state = new State();
        foreach (var pair in new[]
        {
            ("sub", Guid.NewGuid().ToString()),
            (IntegrationClaims.OrganizationId, Guid.NewGuid().ToString()),
            (IntegrationClaims.ClientId, "fic_other")
        })
        {
            var claims = state.Integration().Claims.Select(c => c.Type == pair.Item1 ? new Claim(c.Type, pair.Item2) : c);
            Assert.IsFalse(await state.Validator.IsActiveAsync(Principal(claims), CancellationToken.None));
        }
    }

    [TestMethod]
    public async Task MissingOrDuplicateIntegrationIdentityClaimsAreRejected()
    {
        var state = new State();
        foreach (var type in new[] { IntegrationClaims.ClientId, IntegrationClaims.OrganizationId })
        {
            var token = state.Integration();
            Assert.IsFalse(await state.Validator.IsActiveAsync(
                Principal(token.Claims.Where(c => c.Type != type)), CancellationToken.None));
            Assert.IsFalse(await state.Validator.IsActiveAsync(
                Principal([.. token.Claims, token.FindFirst(type)!]), CancellationToken.None));
        }
    }

    [TestMethod]
    public async Task RequestCancellationIsPropagated()
    {
        var state = new State();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => state.Validator.IsActiveAsync(User(state.UserId), cancellation.Token));
    }

    private static ClaimsPrincipal User(Guid id) => Principal([new("sub", id.ToString())]);
    private static ClaimsPrincipal Principal(IEnumerable<Claim> claims) => new(new ClaimsIdentity(claims, "Bearer"));

    private sealed class State : IUserDirectory, IIntegrationClientRepository, IOrganizationRepository
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public bool UserActive { get; set; } = true;
        public bool UserExists { get; set; } = true;
        public bool ClientExists { get; set; } = true;
        public bool OrganizationExists { get; set; } = true;
        public Organization Organization { get; } = Organization.Create(
            "Test", null, "test", "BG", "EUR", "Europe/Sofia", null, null, DateTimeOffset.UtcNow);
        public IntegrationClient Client { get; }
        public AccessTokenValidator Validator { get; }

        public State()
        {
            Client = IntegrationClient.Create(Organization.Id, "fic_test", "Test", "hash", "prefix",
                "sales.read sales.write", DateTimeOffset.UtcNow);
            Validator = new AccessTokenValidator(this, this, this);
        }

        public ClaimsPrincipal Integration(string scope = "sales.read") => Principal([
            new("sub", Client.Id.ToString()),
            new(IntegrationClaims.ActorType, IntegrationClaims.IntegrationActor),
            new(IntegrationClaims.ClientId, Client.ClientId),
            new(IntegrationClaims.OrganizationId, Organization.Id.ToString()),
            new(IntegrationClaims.Scope, scope)]);

        public Task<IReadOnlyDictionary<Guid, UserDirectoryEntry>> ListByIdsAsync(
            IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<Guid, UserDirectoryEntry> result = UserExists && userIds.Contains(UserId)
                ? new Dictionary<Guid, UserDirectoryEntry> { [UserId] = new(UserId, "test@example.test", "Test", UserActive) }
                : new Dictionary<Guid, UserDirectoryEntry>();
            return Task.FromResult(result);
        }

        public Task<IntegrationClient?> GetAsync(Guid organizationId, Guid integrationClientId,
            bool trackChanges, CancellationToken cancellationToken) => Task.FromResult(
                ClientExists && Client.OrganizationId == organizationId && Client.Id == integrationClientId ? Client : null);
        public Task<Organization?> GetAsync(Guid id, bool trackChanges, CancellationToken cancellationToken) =>
            Task.FromResult(OrganizationExists && Organization.Id == id ? Organization : null);

        public Task<UserDirectoryEntry?> FindByEmailAsync(string email) => throw new NotSupportedException();
        public Task<IReadOnlyList<IntegrationClient>> ListAsync(Guid organizationId, bool includeInactive, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IntegrationClient?> GetByClientIdAsync(string clientId, bool trackChanges, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> ClientIdExistsAsync(string clientId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(IntegrationClient client, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<Organization>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(Organization organization, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
