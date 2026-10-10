using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Fintrox.Application.Identity;
using Fintrox.Contracts.Auth;
using Fintrox.Infrastructure.Identity;
using Fintrox.Infrastructure.Organizations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Identity;

[TestClass]
public sealed class RefreshTokenConcurrencyTests
{
    private string? adminConnection;
    private string? databaseName;
    private string connectionString = null!;
    private readonly Guid userId = Guid.NewGuid();
    private readonly Guid tokenId = Guid.NewGuid();
    private const string OriginalToken = "test-refresh-token";

    [TestInitialize]
    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("FINTROX_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            Assert.Inconclusive("Set FINTROX_TEST_POSTGRES to a disposable PostgreSQL test server (CREATE DATABASE permission required).");
        }

        // Never create, reset or drop the database supplied in the connection string.
        databaseName = "fintrox_auth_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
        await create.ExecuteNonQueryAsync();
        connectionString = new NpgsqlConnectionStringBuilder(adminConnection)
        {
            Database = databaseName,
            Pooling = false
        }.ConnectionString;
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FintroxDbContext>();
        await db.Database.EnsureCreatedAsync();
        var now = DateTimeOffset.UtcNow;
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "test@example.test", Email = "test@example.test",
            DisplayName = "Test", IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = tokenId, UserId = userId, TokenHash = Hash(OriginalToken),
            CreatedAtUtc = now, ExpiresAtUtc = now.AddDays(1)
        });
        await db.SaveChangesAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (databaseName is null || adminConnection is null) return;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }

    [TestMethod]
    public async Task SimultaneousRefreshHasExactlyOneWinnerAndOneReplacement()
    {
        var gate = new ReadGate(2);
        await using var provider = CreateProvider(gate);
        var attempts = new[] { TryRefreshAsync(provider), TryRefreshAsync(provider) };
        await gate.AllRead.Task.WaitAsync(TimeSpan.FromSeconds(20));
        gate.Release.TrySetResult();
        var results = await Task.WhenAll(attempts).WaitAsync(TimeSpan.FromSeconds(20));
        Assert.AreEqual(1, results.Count(result => result is not null));
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FintroxDbContext>();
        var tokens = await db.RefreshTokens.AsNoTracking().ToArrayAsync();
        Assert.HasCount(2, tokens);
        var original = tokens.Single(token => token.Id == tokenId);
        var replacement = tokens.Single(token => token.Id != tokenId);
        Assert.IsNotNull(original.RevokedAtUtc);
        Assert.AreEqual(replacement.Id, original.ReplacedByTokenId);
        Assert.IsNull(replacement.RevokedAtUtc);
        Assert.AreEqual(Hash(results.Single(result => result is not null)!.RefreshToken), replacement.TokenHash);
    }

    [TestMethod]
    public async Task RevokeCommittedWhileRefreshIsReadingPreventsReplacement()
    {
        var gate = new ReadGate(1);
        await using var refreshProvider = CreateProvider(gate);
        var attempt = TryRefreshAsync(refreshProvider);
        await gate.AllRead.Task.WaitAsync(TimeSpan.FromSeconds(20));
        try
        {
            await using var revokeProvider = CreateProvider();
            await using var scope = revokeProvider.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AuthenticationService>()
                .RevokeAsync(new RevokeRefreshTokenRequest(OriginalToken), "revoke", CancellationToken.None);
        }
        finally { gate.Release.TrySetResult(); }
        Assert.IsNull(await attempt.WaitAsync(TimeSpan.FromSeconds(20)));
        await using var checkProvider = CreateProvider();
        await using var check = checkProvider.CreateAsyncScope();
        var tokens = await check.ServiceProvider.GetRequiredService<FintroxDbContext>().RefreshTokens.ToArrayAsync();
        Assert.HasCount(1, tokens);
        Assert.IsNull(tokens[0].ReplacedByTokenId);
        Assert.AreEqual("revoke", tokens[0].RevokedByIp);
    }

    [TestMethod]
    public async Task RevokeAfterRotationPreservesReplacementLinkAndIsIdempotent()
    {
        await using var provider = CreateProvider();
        Assert.IsNotNull(await TryRefreshAsync(provider));
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<AuthenticationService>();
        await service.RevokeAsync(new RevokeRefreshTokenRequest(OriginalToken), "later", CancellationToken.None);
        Assert.IsTrue(await service.RevokeSessionAsync(userId, tokenId, "later", CancellationToken.None));
        var original = await scope.ServiceProvider.GetRequiredService<FintroxDbContext>()
            .RefreshTokens.AsNoTracking().SingleAsync(token => token.Id == tokenId);
        Assert.IsNotNull(original.ReplacedByTokenId);
        Assert.AreEqual("refresh", original.RevokedByIp);
        Assert.IsNull(await TryRefreshAsync(provider));
    }

    [TestMethod]
    public async Task ReplacementSaveFailureRollsBackConsumptionAndAllowsRetryInSameContext()
    {
        var failure = new FailReplacementOnce();
        await using var provider = CreateProvider(failure);
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<AuthenticationService>();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => service.RefreshAsync(
            new RefreshTokenRequest(OriginalToken), "refresh", null, CancellationToken.None));
        var db = scope.ServiceProvider.GetRequiredService<FintroxDbContext>();
        var original = await db.RefreshTokens.AsNoTracking().SingleAsync();
        Assert.IsNull(original.RevokedAtUtc);
        Assert.IsNull(original.ReplacedByTokenId);
        Assert.IsFalse(db.ChangeTracker.Entries<RefreshToken>().Any(entry => entry.State == EntityState.Added));
        Assert.IsNotNull(await service.RefreshAsync(new RefreshTokenRequest(OriginalToken), "refresh", null, CancellationToken.None));
        Assert.AreEqual(2, await db.RefreshTokens.CountAsync());
    }

    [TestMethod]
    public async Task SessionRevocationRequiresOwnerAndPreventsRefresh()
    {
        await using var provider = CreateProvider();
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<AuthenticationService>();
        Assert.IsFalse(await service.RevokeSessionAsync(Guid.NewGuid(), tokenId, null, CancellationToken.None));
        Assert.IsTrue(await service.RevokeSessionAsync(userId, tokenId, "owner", CancellationToken.None));
        Assert.IsTrue(await service.RevokeSessionAsync(userId, tokenId, "again", CancellationToken.None));
        Assert.IsNull(await TryRefreshAsync(provider));
        Assert.AreEqual(1, await scope.ServiceProvider.GetRequiredService<FintroxDbContext>().RefreshTokens.CountAsync());
    }

    private ServiceProvider CreateProvider(params IInterceptor[] interceptors)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<FintroxDbContext>(options => options.UseNpgsql(connectionString,
            postgres => postgres.EnableRetryOnFailure()).AddInterceptors(interceptors));
        services.AddIdentityCore<ApplicationUser>().AddEntityFrameworkStores<FintroxDbContext>();
        services.AddScoped<AuthenticationService>(provider => new AuthenticationService(
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<FintroxDbContext>(), new TestAccessTokens(),
            new OrganizationMembershipRepository(provider.GetRequiredService<FintroxDbContext>()), TimeProvider.System));
        return services.BuildServiceProvider();
    }

    private static async Task<AuthTokenResponse?> TryRefreshAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        try
        {
            return await scope.ServiceProvider.GetRequiredService<AuthenticationService>()
                .RefreshAsync(new RefreshTokenRequest(OriginalToken), "refresh", null, CancellationToken.None);
        }
        catch (AuthenticationException) { return null; }
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private sealed class TestAccessTokens : IAccessTokenService
    {
        public AccessToken Create(Guid userId, string email, string displayName) => new("test-access", DateTimeOffset.UtcNow.AddMinutes(15));
    }

    private sealed class ReadGate(int participants) : DbCommandInterceptor
    {
        private int arrivals;
        public TaskCompletionSource AllRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("SELECT", StringComparison.Ordinal)
                && command.CommandText.Contains("refresh_tokens", StringComparison.Ordinal)
                && !Release.Task.IsCompleted)
            {
                if (Interlocked.Increment(ref arrivals) == participants) AllRead.TrySetResult();
                await Release.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
            return result;
        }
    }

    private sealed class FailReplacementOnce : SaveChangesInterceptor
    {
        private bool failed;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!failed && eventData.Context!.ChangeTracker.Entries<RefreshToken>().Any(entry => entry.State == EntityState.Added))
            {
                failed = true;
                throw new InvalidOperationException("Injected replacement-save failure.");
            }
            return ValueTask.FromResult(result);
        }
    }
}
