using Fintrox.Application.Authorization;
using Fintrox.Application.Common;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Identity;
using Fintrox.Application.Organizations;
using Fintrox.Contracts.Organizations;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Identity;
using Fintrox.Infrastructure.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Organizations;

[TestClass]
public sealed class OrganizationOwnerConcurrencyTests
{
    private string? adminConnection;
    private string? databaseName;
    private string connectionString = null!;
    private Guid organizationId;
    private readonly Guid firstOwnerId = Guid.NewGuid();
    private readonly Guid secondOwnerId = Guid.NewGuid();
    private readonly Guid newMemberId = Guid.NewGuid();

    [TestInitialize]
    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("FINTROX_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            Assert.Inconclusive(
                "Set FINTROX_TEST_POSTGRES to a disposable PostgreSQL server with CREATE DATABASE permission.");
        }

        databaseName = "fintrox_owners_test_" + Guid.NewGuid().ToString("N");
        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        connectionString = new NpgsqlConnectionStringBuilder(adminConnection)
        {
            Database = databaseName,
            Pooling = false
        }.ConnectionString;

        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        var now = DateTimeOffset.UtcNow;
        var organization = Organization.Create(
            "Example", null, "example", "BG", "EUR", "Europe/Sofia",
            null, null, now);
        organizationId = organization.Id;
        db.Organizations.Add(organization);
        db.Users.AddRange(
            User(firstOwnerId, "owner-a@example.test", now),
            User(secondOwnerId, "owner-b@example.test", now),
            User(newMemberId, "member-c@example.test", now));
        db.OrganizationMemberships.AddRange(
            OrganizationMembership.Create(
                organizationId, firstOwnerId, OrganizationRole.Owner, now),
            OrganizationMembership.Create(
                organizationId, secondOwnerId, OrganizationRole.Owner, now));
        await db.SaveChangesAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (adminConnection is null || databaseName is null) return;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }

    [TestMethod]
    public async Task TwoConcurrentSelfDeactivationsCannotRemoveBothOwners()
    {
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = TryDeactivateAsync(firstOwnerId, start.Task);
        var second = TryDeactivateAsync(secondOwnerId, start.Task);
        start.SetResult();

        var outcomes = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is null));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is OrganizationMembershipConflictException));
        await AssertExactlyOneActiveOwnerAsync();
    }

    [TestMethod]
    public async Task TwoConcurrentOwnerDemotionsCannotRemoveBothOwners()
    {
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = TryDemoteAsync(firstOwnerId, start.Task);
        var second = TryDemoteAsync(secondOwnerId, start.Task);
        start.SetResult();

        var outcomes = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is null));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is OrganizationMembershipConflictException));
        await AssertExactlyOneActiveOwnerAsync();
    }

    [TestMethod]
    public async Task ParallelAdditionOfSameMemberHasOneWinner()
    {
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = TryAddAsync(firstOwnerId, start.Task);
        var second = TryAddAsync(secondOwnerId, start.Task);
        start.SetResult();

        var outcomes = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is null));
        Assert.AreEqual(1, outcomes.Count(outcome => outcome is OrganizationMembershipConflictException));
        await using var db = CreateDb();
        Assert.AreEqual(1, await db.OrganizationMemberships.AsNoTracking().CountAsync(
            m => m.OrganizationId == organizationId && m.UserId == newMemberId));
        Assert.AreEqual(2, await db.OrganizationMemberships.AsNoTracking().CountAsync(
            m => m.OrganizationId == organizationId && m.IsActive && m.Role == OrganizationRole.Owner));
    }

    [TestMethod]
    public async Task LockRequiresTransactionAndIsReleasedOnCommit()
    {
        await using var firstDb = CreateDb();
        var firstRepo = new OrganizationMembershipRepository(firstDb);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            firstRepo.LockOrganizationForMembershipChangeAsync(organizationId, CancellationToken.None));

        await using var firstTx = await firstDb.Database.BeginTransactionAsync();
        await firstRepo.LockOrganizationForMembershipChangeAsync(organizationId, CancellationToken.None);

        await using var secondDb = CreateDb();
        await using var secondTx = await secondDb.Database.BeginTransactionAsync();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLock = Task.Run(async () =>
        {
            started.SetResult();
            await new OrganizationMembershipRepository(secondDb)
                .LockOrganizationForMembershipChangeAsync(organizationId, CancellationToken.None);
        });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Task.Delay(150);
        Assert.IsFalse(secondLock.IsCompleted, "Second transaction must wait on the row lock.");

        await firstTx.CommitAsync();
        await secondLock.WaitAsync(TimeSpan.FromSeconds(10));
        await secondTx.CommitAsync();
    }

    private async Task<Exception?> TryDeactivateAsync(Guid userId, Task start)
    {
        await start;
        await using var db = CreateDb();
        try
        {
            return await Service(db, userId).DeactivateAsync(
                organizationId, userId, CancellationToken.None) ? null
                : new InvalidOperationException("Owner membership missing.");
        }
        catch (OrganizationMembershipConflictException exception) { return exception; }
    }

    private async Task<Exception?> TryDemoteAsync(Guid userId, Task start)
    {
        await start;
        await using var db = CreateDb();
        try
        {
            return await Service(db, userId).ChangeRoleAsync(
                organizationId, userId,
                new UpdateOrganizationMemberRoleRequest("Administrator"),
                CancellationToken.None) is not null ? null
                : new InvalidOperationException("Owner membership missing.");
        }
        catch (OrganizationMembershipConflictException exception) { return exception; }
    }

    private async Task<Exception?> TryAddAsync(Guid userId, Task start)
    {
        await start;
        await using var db = CreateDb();
        try
        {
            await Service(db, userId).AddAsync(
                organizationId,
                new AddOrganizationMemberRequest("member-c@example.test", "Viewer"),
                CancellationToken.None);
            return null;
        }
        catch (OrganizationMembershipConflictException exception) { return exception; }
    }

    private OrganizationMemberService Service(FintroxDbContext db, Guid userId)
    {
        var members = new OrganizationMembershipRepository(db);
        var organizations = new OrganizationRepository(db);
        return new OrganizationMemberService(
            members, new OrganizationAccessService(members, organizations),
            new Directory(firstOwnerId, secondOwnerId, newMemberId),
            new UserContext(userId), TimeProvider.System, new EfTransactionRunner(db));
    }

    private async Task AssertExactlyOneActiveOwnerAsync()
    {
        await using var db = CreateDb();
        Assert.AreEqual(1, await db.OrganizationMemberships.AsNoTracking()
            .CountAsync(m => m.OrganizationId == organizationId && m.IsActive &&
                             m.Role == OrganizationRole.Owner));
    }

    private FintroxDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FintroxDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
            .Options;
        return new FintroxDbContext(options);
    }

    private static ApplicationUser User(Guid id, string email, DateTimeOffset now) => new()
    {
        Id = id, UserName = email, Email = email, DisplayName = email,
        IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now
    };

    private sealed class UserContext(Guid userId) : ICurrentUser
    {
        public Guid? UserId => userId;
        public Guid RequireUserId() => userId;
    }

    private sealed class Directory(Guid a, Guid b, Guid c) : IUserDirectory
    {
        private readonly IReadOnlyDictionary<Guid, UserDirectoryEntry> users =
            new Dictionary<Guid, UserDirectoryEntry>
            {
                [a] = new(a, "owner-a@example.test", "Owner A", true),
                [b] = new(b, "owner-b@example.test", "Owner B", true),
                [c] = new(c, "member-c@example.test", "Member C", true)
            };

        public Task<UserDirectoryEntry?> FindByEmailAsync(string email) =>
            Task.FromResult(users.Values.FirstOrDefault(u =>
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyDictionary<Guid, UserDirectoryEntry>> ListByIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<Guid, UserDirectoryEntry> selection =
                users.Where(pair => userIds.Contains(pair.Key))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            return Task.FromResult(selection);
        }
    }
}
