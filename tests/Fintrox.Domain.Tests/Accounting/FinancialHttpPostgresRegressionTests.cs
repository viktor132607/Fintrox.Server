using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrox.Application.Identity;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Identity;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Actual HTTPS-in-memory ASP.NET Core requests against a disposable PostgreSQL
/// database with the repository's real EF migrations and journal triggers.
/// </summary>
[TestClass]
public sealed class FinancialHttpPostgresRegressionTests
{
    private const string Endpoint = "/api/v1/accounting/journal";
    private const string TestKey = "TEST_ONLY_financial_http_postgres_signing_key_very_long_and_random_123456";
    private static readonly DateOnly PostingDate = new(2026, 3, 15);
    private string? adminConnection;
    private string? databaseName;
    private string connectionString = null!;
    private WebApplicationFactory<Program>? factory;
    private Guid organizationA;
    private Guid organizationB;
    private readonly Guid userA = Guid.NewGuid();
    private readonly Guid userB = Guid.NewGuid();
    private Guid assetA;
    private Guid revenueA;
    private Guid assetB;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("FINTROX_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            Assert.Inconclusive("Set FINTROX_TEST_POSTGRES to a disposable PostgreSQL server with CREATE DATABASE permission.");
        }

        // Never drop or modify the admin database: each test owns a unique database.
        databaseName = "fintrox_fin_http_" + Guid.NewGuid().ToString("N");
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

        await using (var db = CreateDb())
        {
            await db.Database.MigrateAsync();
            var now = DateTimeOffset.UtcNow;

            var a = Organization.Create("Company A", null, "financial-test-a", "BG",
                "EUR", "Europe/Sofia", null, null, now);
            var b = Organization.Create("Company B", null, "financial-test-b", "BG",
                "EUR", "Europe/Sofia", null, null, now);
            organizationA = a.Id;
            organizationB = b.Id;
            db.Organizations.AddRange(a, b);
            db.Users.AddRange(CreateUser(userA, "finance-a@example.test", now),
                CreateUser(userB, "finance-b@example.test", now));
            db.OrganizationMemberships.AddRange(
                OrganizationMembership.Create(organizationA, userA, OrganizationRole.Owner, now),
                OrganizationMembership.Create(organizationB, userB, OrganizationRole.Owner, now));

            var yearA = FiscalYear.Create(organizationA, "FY 2026", new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31), now);
            var yearB = FiscalYear.Create(organizationB, "FY 2026", new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31), now);
            db.FiscalYears.AddRange(yearA, yearB);

            db.AccountingPeriods.AddRange(
                AccountingPeriod.Create(organizationA, yearA.Id, 3, "March",
                    new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), now),
                AccountingPeriod.Create(organizationB, yearB.Id, 3, "March",
                    new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), now));

            var debitA = Account.Create(organizationA, "501", "Cash A",
                AccountType.Asset, null, true, true, now);
            var creditA = Account.Create(organizationA, "701", "Sales A",
                AccountType.Revenue, null, true, true, now);
            var debitB = Account.Create(organizationB, "501", "Cash B",
                AccountType.Asset, null, true, true, now);
            assetA = debitA.Id;
            revenueA = creditA.Id;
            assetB = debitB.Id;
            db.Accounts.AddRange(debitA, creditA, debitB);
            await db.SaveChangesAsync();
        }

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,
                    ["Jwt:SigningKey"] = TestKey,
                    ["Jwt:Issuer"] = "Fintrox",
                    ["Jwt:Audience"] = "Fintrox.Client"
                }));
            builder.ConfigureTestServices(services =>
            {
                // Background webhook delivery is unrelated to finance HTTP testing.
                var worker = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService) &&
                    descriptor.ImplementationType?.Name == "WebhookDeliveryWorker");
                if (worker is not null) services.Remove(worker);
            });
        });
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (factory is not null) await factory.DisposeAsync();
        if (databaseName is null || adminConnection is null) return;
        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }

    [TestMethod]
    public async Task HttpPostingProducesBalancedPersistedDoubleEntryAndNumber()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 125.0123m, 125.0123m);
        Assert.AreEqual("Draft", draft.Status);
        Assert.IsTrue(draft.IsBalanced);

        using var response = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var posted = await response.Content.ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(posted);
        Assert.AreEqual("Posted", posted.Status);
        Assert.IsTrue(posted.IsBalanced);
        Assert.AreEqual(125.0123m, posted.DebitTotal);
        Assert.AreEqual(125.0123m, posted.CreditTotal);
        Assert.IsNotNull(posted.Number);
        Assert.IsNotNull(posted.PostedAtUtc);

        await using var db = CreateDb();
        var stored = await db.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == draft.Id);
        Assert.AreEqual(JournalEntryStatus.Posted, stored.Status);
        Assert.AreEqual(posted.Number, stored.Number);
        var lines = await db.JournalLines.AsNoTracking()
            .Where(x => x.JournalEntryId == draft.Id).ToArrayAsync();
        Assert.HasCount(2, lines);
        Assert.AreEqual(lines.Sum(x => x.Debit), lines.Sum(x => x.Credit));
        Assert.AreEqual(1, await db.JournalNumberSequences.AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task UnbalancedDraftIsRejectedWithoutConsumingPostingSequence()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 100m, 90m);
        Assert.IsFalse(draft.IsBalanced);

        using var response = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(JournalEntryStatus.Draft,
            (await db.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == draft.Id)).Status);
        Assert.AreEqual(0, await db.JournalNumberSequences.CountAsync());
    }

    [TestMethod]
    public async Task HttpReversalSwapsDebitCreditAndCannotBeAppliedTwice()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 47m, 47m);
        using var postedResponse = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        Assert.AreEqual(HttpStatusCode.OK, postedResponse.StatusCode);

        using var response = await client.PostAsJsonAsync($"{Endpoint}/{draft.Id}/reverse",
            new ReverseJournalEntryRequest(PostingDate, "Correcting original entry"));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var reversal = await response.Content.ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(reversal);
        Assert.AreEqual("Posted", reversal.Status);
        Assert.AreEqual(draft.Id, reversal.ReversalOfJournalEntryId);
        Assert.AreEqual(47m, reversal.CreditTotal);
        Assert.AreEqual(47m, reversal.DebitTotal);
        Assert.IsTrue(reversal.IsBalanced);

        await using (var db = CreateDb())
        {
            var original = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == draft.Id);
            Assert.AreEqual(JournalEntryStatus.Reversed, original.Status);
            Assert.AreEqual(reversal.Id, original.ReversedByJournalEntryId);
            var sourceLines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == draft.Id).OrderBy(x => x.LineNumber).ToArrayAsync();
            var reversedLines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == reversal.Id).OrderBy(x => x.LineNumber).ToArrayAsync();
            Assert.HasCount(2, reversedLines);
            for (var index = 0; index < sourceLines.Length; index++)
            {
                Assert.AreEqual(sourceLines[index].Debit, reversedLines[index].Credit);
                Assert.AreEqual(sourceLines[index].Credit, reversedLines[index].Debit);
            }
        }

        using var again = await client.PostAsJsonAsync($"{Endpoint}/{draft.Id}/reverse",
            new ReverseJournalEntryRequest(PostingDate, "Duplicate"));
        Assert.AreEqual(HttpStatusCode.Conflict, again.StatusCode);
        await using var verify = CreateDb();
        Assert.AreEqual(2, await verify.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task ClosedPeriodRejectsPostingAndReversalWithoutLedgerMutation()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 25m, 25m);
        var postedDraft = await CreateDraftAsync(client, 10m, 10m);
        using (var posting = await client.PostAsync($"{Endpoint}/{postedDraft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, posting.StatusCode);

        await using (var db = CreateDb())
        {
            var period = await db.AccountingPeriods.SingleAsync(x => x.OrganizationId == organizationA);
            period.Close(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using var post = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        Assert.AreEqual(HttpStatusCode.Conflict, post.StatusCode);
        using var reverse = await client.PostAsJsonAsync($"{Endpoint}/{postedDraft.Id}/reverse",
            new ReverseJournalEntryRequest(PostingDate, "After period close"));
        Assert.AreEqual(HttpStatusCode.Conflict, reverse.StatusCode);

        await using var check = CreateDb();
        Assert.AreEqual(JournalEntryStatus.Draft,
            (await check.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == draft.Id)).Status);
        Assert.AreEqual(JournalEntryStatus.Posted,
            (await check.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == postedDraft.Id)).Status);
        Assert.AreEqual(1, await check.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA && x.Status == JournalEntryStatus.Posted));
        Assert.AreEqual(1, await check.JournalNumberSequences.CountAsync());
    }

    [TestMethod]
    public async Task HttpRejectsCrossTenantAccountAndJournalAccess()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userB, organizationB);

        var firstDraft = await CreateDraftAsync(first, 30m, 30m);
        using var leak = await second.GetAsync($"{Endpoint}/{firstDraft.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, leak.StatusCode);
        using var leakPost = await second.PostAsync($"{Endpoint}/{firstDraft.Id}/post", null);
        Assert.AreEqual(HttpStatusCode.NotFound, leakPost.StatusCode);
        using var leakReverse = await second.PostAsJsonAsync($"{Endpoint}/{firstDraft.Id}/reverse",
            new ReverseJournalEntryRequest(PostingDate, "Wrong tenant"));
        Assert.AreEqual(HttpStatusCode.NotFound, leakReverse.StatusCode);

        using var invalid = await first.PostAsJsonAsync(Endpoint,
            new CreateJournalEntryRequest(PostingDate, PostingDate,
                "Reject other company's account", null,
                [new CreateJournalLineRequest(assetB, 30m, 0m, null),
                 new CreateJournalLineRequest(revenueA, 0m, 30m, null)]));
        Assert.AreEqual(HttpStatusCode.Conflict, invalid.StatusCode);

        // A valid tenant header is not sufficient without tenant membership.
        using var spoofed = Client(userB, organizationA);
        using var deny = await spoofed.GetAsync(Endpoint);
        Assert.AreEqual(HttpStatusCode.Forbidden, deny.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync());
        Assert.AreEqual(0, await db.JournalLines.CountAsync(x => x.AccountId == assetB));
    }

    [TestMethod]
    public async Task HttpRequiresValidBearerIdentityAndTenant()
    {
        using var anonymous = factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        anonymous.DefaultRequestHeaders.Add("X-Organization-Id", organizationA.ToString());
        using var unauthorized = await anonymous.GetAsync(Endpoint);
        Assert.AreEqual(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        using var noTenant = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        noTenant.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", Token(userA));
        using var forbidden = await noTenant.GetAsync(Endpoint);
        Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [TestMethod]
    public async Task PostedLedgerLinesAreProtectedByRealPostgresTrigger()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 25m, 25m);
        using (var post = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, post.StatusCode);

        await using var db = CreateDb();
        await Assert.ThrowsExactlyAsync<PostgresException>(async () =>
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM accounting.journal_lines WHERE organization_id = {organizationA} AND journal_entry_id = {draft.Id}"));

        await using var verify = CreateDb();
        Assert.AreEqual(2, await verify.JournalLines.CountAsync(x => x.JournalEntryId == draft.Id));
    }

    private HttpClient Client(Guid userId, Guid organizationId)
    {
        var client = factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token(userId));
        client.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
        return client;
    }

    private string Token(Guid userId) => factory!.Services
        .GetRequiredService<IAccessTokenService>()
        .Create(userId, $"user-{userId}@example.test", "Finance tester").Value;

    private async Task<JournalEntryResponse> CreateDraftAsync(
        HttpClient client, decimal debit, decimal credit)
    {
        using var response = await client.PostAsJsonAsync(Endpoint,
            new CreateJournalEntryRequest(PostingDate, PostingDate,
                "HTTP financial regression", null,
                [new CreateJournalLineRequest(assetA, debit, 0m, "Debit"),
                 new CreateJournalLineRequest(revenueA, 0m, credit, "Credit")]));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            "WWW-Authenticate: " + string.Join("; ", response.Headers.WwwAuthenticate)
            + " Body: " + await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(result);
        return result;
    }

    private FintroxDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FintroxDbContext>()
            .UseNpgsql(connectionString).Options;
        return new FintroxDbContext(options);
    }

    private static ApplicationUser CreateUser(Guid id, string email, DateTimeOffset now) => new()
    {
        Id = id, Email = email, UserName = email, DisplayName = email,
        IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now
    };
}
