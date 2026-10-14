using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintrox.Application.Identity;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Identity;
using Fintrox.Infrastructure.Audit;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    private Guid equityA;
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
            var capitalA = Account.Create(organizationA, "101", "Share capital A",
                AccountType.Equity, null, true, true, now);
            assetA = debitA.Id;
            revenueA = creditA.Id;
            equityA = capitalA.Id;
            assetB = debitB.Id;
            db.Accounts.AddRange(debitA, creditA, debitB, capitalA);
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
                // Override DbContext options registered by the real API host:
                // test app and fixture must use the SAME migrated disposable DB.
                services.RemoveAll<DbContextOptions<FintroxDbContext>>();
                services.AddDbContext<FintroxDbContext>((provider, options) =>
                {
                    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
                    options.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());
                });

                // Background webhook delivery is unrelated to finance HTTP testing.
                var worker = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService) &&
                    descriptor.ImplementationType?.Name == "WebhookDeliveryWorker");
                if (worker is not null) services.Remove(worker);
            });
        });

        using var verifyScope = factory.Services.CreateScope();
        var configuredDb = verifyScope.ServiceProvider.GetRequiredService<FintroxDbContext>();
        Assert.AreEqual(databaseName, configuredDb.Database.GetDbConnection().Database,
            "The HTTP host must use the migrated disposable test database.");
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

    [TestMethod]
    public async Task ConcurrentPostOfSameDraftProducesOneNumberAndOneWinner()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var draft = await CreateDraftAsync(first, 150m, 150m);

        var attempts = await Task.WhenAll(
            first.PostAsync($"{Endpoint}/{draft.Id}/post", null),
            second.PostAsync($"{Endpoint}/{draft.Id}/post", null));

        try
        {
            Assert.AreEqual(1, attempts.Count(x => x.StatusCode == HttpStatusCode.OK));
            Assert.AreEqual(1, attempts.Count(x => x.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var attempt in attempts) attempt.Dispose();
        }

        await using var db = CreateDb();
        var posted = await db.JournalEntries.AsNoTracking()
            .SingleAsync(x => x.Id == draft.Id && x.OrganizationId == organizationA);
        Assert.AreEqual(JournalEntryStatus.Posted, posted.Status);
        Assert.AreEqual(1, await db.JournalNumberSequences.CountAsync(x =>
            x.OrganizationId == organizationA && x.LastNumber == 1));
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.Id == draft.Id && x.Status == JournalEntryStatus.Posted));
    }

    [TestMethod]
    public async Task ConcurrentReverseOfSameJournalProducesOneReversal()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var draft = await CreateDraftAsync(first, 75m, 75m);
        using (var post = await first.PostAsync($"{Endpoint}/{draft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, post.StatusCode);

        var url = $"{Endpoint}/{draft.Id}/reverse";
        var reversals = await Task.WhenAll(
            first.PostAsJsonAsync(url, new ReverseJournalEntryRequest(
                PostingDate, "Fix original entry A")),
            second.PostAsJsonAsync(url, new ReverseJournalEntryRequest(
                PostingDate, "Fix original entry B")));

        try
        {
            Assert.AreEqual(1, reversals.Count(x => x.StatusCode == HttpStatusCode.Created));
            Assert.AreEqual(1, reversals.Count(x => x.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var reversal in reversals) reversal.Dispose();
        }

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA && x.ReversalOfJournalEntryId == draft.Id));
        Assert.AreEqual(2L, await db.JournalNumberSequences
            .Where(x => x.OrganizationId == organizationA)
            .Select(x => x.LastNumber).SingleAsync());
    }

    [TestMethod]
    public async Task PostingWaitsForPeriodClosureAndRejectsStaleOpenStatus()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 63m, 63m);

        await using var locker = new NpgsqlConnection(connectionString);
        await locker.OpenAsync();
        await using var transaction = await locker.BeginTransactionAsync();
        await using (var close = new NpgsqlCommand(
            "UPDATE accounting.accounting_periods SET status = 'Closed' " +
            "WHERE organization_id = @organization AND id = @period", locker, transaction))
        {
            close.Parameters.AddWithValue("organization", organizationA);
            close.Parameters.AddWithValue("period", draft.FiscalPeriodId);
            Assert.AreEqual(1, await close.ExecuteNonQueryAsync());
        }

        var pending = client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        await Task.Delay(250);
        await transaction.CommitAsync();

        using var response = await pending;
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(JournalEntryStatus.Draft,
            (await db.JournalEntries.AsNoTracking().SingleAsync(x => x.Id == draft.Id)).Status);
        Assert.AreEqual(0, await db.JournalNumberSequences.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task PostingWaitsForFiscalYearClosureAndRejectsStaleOpenStatus()
    {
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 80m, 80m);
        await using var db = CreateDb();
        var period = await db.AccountingPeriods.AsNoTracking()
            .SingleAsync(x => x.Id == draft.FiscalPeriodId);

        await using var locker = new NpgsqlConnection(connectionString);
        await locker.OpenAsync();
        await using var transaction = await locker.BeginTransactionAsync();
        await using (var close = new NpgsqlCommand(
            "UPDATE accounting.fiscal_years SET status = 'SoftClosed' " +
            "WHERE organization_id = @organization AND id = @year", locker, transaction))
        {
            close.Parameters.AddWithValue("organization", organizationA);
            close.Parameters.AddWithValue("year", period.FiscalYearId);
            Assert.AreEqual(1, await close.ExecuteNonQueryAsync());
        }

        var pending = client.PostAsync($"{Endpoint}/{draft.Id}/post", null);
        await Task.Delay(250);
        await transaction.CommitAsync();

        using var response = await pending;
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual(0, await db.JournalNumberSequences.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task OpeningBalancesImportIsBalancedPostedAndIdempotentOnReplay()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        var request = ValidOpeningBalances(fiscalYearId);
        using var client = Client(userA, organizationA);

        using var first = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import", request);
        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode,
            await first.Content.ReadAsStringAsync());
        var original = await first.Content.ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(original);
        Assert.AreEqual("Posted", original.Status);
        Assert.AreEqual("System", original.Source);
        Assert.IsTrue(original.IsBalanced);
        Assert.AreEqual(350m, original.DebitTotal);
        Assert.AreEqual(350m, original.CreditTotal);
        Assert.AreEqual(new DateOnly(2026, 1, 1), original.PostingDate);

        using var replay = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import", request);
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var duplicate = await replay.Content.ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(duplicate);
        Assert.AreEqual(original.Id, duplicate.Id);
        Assert.AreEqual(original.Number, duplicate.Number);

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
        Assert.AreEqual(2, await db.JournalLines.CountAsync(x =>
            x.OrganizationId == organizationA));
        Assert.AreEqual(1L, await db.JournalNumberSequences.Where(x =>
            x.OrganizationId == organizationA).Select(x => x.LastNumber).SingleAsync());
    }

    [TestMethod]
    public async Task OpeningBalancesRejectInvalidAndForeignAccountWithoutAllocatingNumber()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var unbalanced = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            new ImportOpeningBalancesRequest(fiscalYearId,
            [
                new CreateJournalLineRequest(assetA, 350m, 0m, null),
                new CreateJournalLineRequest(equityA, 0m, 300m, null)
            ]));
        Assert.AreEqual(HttpStatusCode.Conflict, unbalanced.StatusCode);

        using var foreign = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            new ImportOpeningBalancesRequest(fiscalYearId,
            [
                new CreateJournalLineRequest(assetB, 350m, 0m, null),
                new CreateJournalLineRequest(equityA, 0m, 350m, null)
            ]));
        Assert.AreEqual(HttpStatusCode.Conflict, foreign.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
        Assert.AreEqual(0, await db.JournalNumberSequences.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task OpeningBalancesCannotBeInsertedAfterOrdinaryLedgerPosting()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 25m, 25m);
        using (var posted = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, posted.StatusCode);

        using var response = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(fiscalYearId));
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
        Assert.AreEqual(1L, await db.JournalNumberSequences.Where(x =>
            x.OrganizationId == organizationA).Select(x => x.LastNumber).SingleAsync());
    }

    [TestMethod]
    public async Task OpeningBalancesChangedReplayIsRejectedEvenAfterInitialSuccess()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        using var client = Client(userA, organizationA);
        using (var initial = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(fiscalYearId)))
            Assert.AreEqual(HttpStatusCode.OK, initial.StatusCode);

        using var changed = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            new ImportOpeningBalancesRequest(fiscalYearId,
            [
                new CreateJournalLineRequest(assetA, 400m, 0m, null),
                new CreateJournalLineRequest(equityA, 0m, 400m, null)
            ]));
        Assert.AreEqual(HttpStatusCode.Conflict, changed.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task OpeningBalancesAreTenantScopedAndRequireAuthentication()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        using var otherTenant = Client(userB, organizationB);
        using var crossTenant = await otherTenant.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(fiscalYearId));
        Assert.AreEqual(HttpStatusCode.Conflict, crossTenant.StatusCode);

        using var anonymous = factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var unauthenticated = await anonymous.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(fiscalYearId));
        Assert.AreEqual(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA || x.OrganizationId == organizationB));
    }

    [TestMethod]
    public async Task ConcurrentOpeningBalancesReplaysHaveOnlyOneLedgerEffect()
    {
        var fiscalYearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        var request = ValidOpeningBalances(fiscalYearId);
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);

        var attempts = await Task.WhenAll(
            first.PostAsJsonAsync("/api/v1/accounting/opening-balances/import", request),
            second.PostAsJsonAsync("/api/v1/accounting/opening-balances/import", request));
        try
        {
            Assert.IsTrue(attempts.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join("; ", await Task.WhenAll(
                    attempts.Select(x => x.Content.ReadAsStringAsync()))));
            var posted = await Task.WhenAll(
                attempts.Select(x => x.Content.ReadFromJsonAsync<JournalEntryResponse>()));
            Assert.IsNotNull(posted[0]);
            Assert.IsNotNull(posted[1]);
            Assert.AreEqual(posted[0].Id, posted[1].Id);
        }
        finally
        {
            foreach (var attempt in attempts) attempt.Dispose();
        }

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
        Assert.AreEqual(1L, await db.JournalNumberSequences.Where(x =>
            x.OrganizationId == organizationA).Select(x => x.LastNumber).SingleAsync());
    }

    [TestMethod]
    public async Task OpeningBalancesRejectRevenueAndExpenseAccounts()
    {
        var yearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            new ImportOpeningBalancesRequest(yearId,
            [
                new CreateJournalLineRequest(assetA, 50m, 0m, null),
                new CreateJournalLineRequest(revenueA, 0m, 50m, null)
            ]));
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task OpeningBalancesRequireOpenFiscalYearAndFirstAccountingPeriod()
    {
        var yearId = await AddJanuaryOpeningPeriodAsync(organizationA);
        await using (var db = CreateDb())
        {
            var period = await db.AccountingPeriods.SingleAsync(x =>
                x.OrganizationId == organizationA && x.StartDate == new DateOnly(2026, 1, 1));
            period.Close(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using var client = Client(userA, organizationA);
        using var periodRejected = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(yearId));
        Assert.AreEqual(HttpStatusCode.Conflict, periodRejected.StatusCode);

        await using (var db = CreateDb())
        {
            var period = await db.AccountingPeriods.SingleAsync(x =>
                x.OrganizationId == organizationA && x.StartDate == new DateOnly(2026, 1, 1));
            period.Reopen(DateTimeOffset.UtcNow);
            var year = await db.FiscalYears.SingleAsync(x => x.Id == yearId);
            year.SoftClose(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using var yearRejected = await client.PostAsJsonAsync(
            "/api/v1/accounting/opening-balances/import",
            ValidOpeningBalances(yearId));
        Assert.AreEqual(HttpStatusCode.Conflict, yearRejected.StatusCode);

        await using var verify = CreateDb();
        Assert.AreEqual(0, await verify.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task PeriodCloseRejectsDraftThenAllowsCloseAfterPosting()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 100m, 100m);
        var url = $"/api/v1/accounting/fiscal-years/{calendar.YearId}/periods/{calendar.PeriodId}";

        using (var rejected = await client.PostAsync($"{url}/close", null))
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);

        await using (var db = CreateDb())
            Assert.AreEqual(AccountingPeriodStatus.Open,
                (await db.AccountingPeriods.AsNoTracking()
                    .SingleAsync(x => x.Id == calendar.PeriodId)).Status);

        using (var posted = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, posted.StatusCode);

        using (var closed = await client.PostAsync($"{url}/close", null))
        {
            Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode,
                await closed.Content.ReadAsStringAsync());
            var period = await closed.Content.ReadFromJsonAsync<AccountingPeriodResponse>();
            Assert.IsNotNull(period);
            Assert.AreEqual("Closed", period.Status);
        }

        // New drafts cannot slip into a closed calendar; the creation path
        // uses the same transactionally held calendar row locks.
        using var create = await client.PostAsJsonAsync(Endpoint,
            new CreateJournalEntryRequest(PostingDate, PostingDate, "Late draft", null,
            [
                new CreateJournalLineRequest(assetA, 9m, 0m, null),
                new CreateJournalLineRequest(revenueA, 0m, 9m, null)
            ]));
        Assert.AreEqual(HttpStatusCode.Conflict, create.StatusCode);

        await using var verify = CreateDb();
        Assert.AreEqual(1, await verify.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task SoftClosedPeriodIsReopenableAndBlocksNewDrafts()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 30m, 30m);
        var url = $"/api/v1/accounting/fiscal-years/{calendar.YearId}/periods/{calendar.PeriodId}";

        using (var rejected = await client.PostAsync($"{url}/soft-close", null))
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
        using (var posted = await client.PostAsync($"{Endpoint}/{draft.Id}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, posted.StatusCode);
        using (var softClosed = await client.PostAsync($"{url}/soft-close", null))
        {
            Assert.AreEqual(HttpStatusCode.OK, softClosed.StatusCode);
            Assert.AreEqual("SoftClosed",
                (await softClosed.Content.ReadFromJsonAsync<AccountingPeriodResponse>())!.Status);
        }

        using (var late = await client.PostAsync(Endpoint,
            JsonContent.Create(new CreateJournalEntryRequest(
                PostingDate, PostingDate, "Late", null, null))))
            Assert.AreEqual(HttpStatusCode.Conflict, late.StatusCode);

        using (var reopened = await client.PostAsync($"{url}/reopen", null))
        {
            Assert.AreEqual(HttpStatusCode.OK, reopened.StatusCode);
            Assert.AreEqual("Open",
                (await reopened.Content.ReadFromJsonAsync<AccountingPeriodResponse>())!.Status);
        }
    }

    [TestMethod]
    public async Task FiscalYearCannotCloseBeforeAllPeriodsClose()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var yearUrl = $"/api/v1/accounting/fiscal-years/{calendar.YearId}";
        var periodUrl = $"{yearUrl}/periods/{calendar.PeriodId}";

        using (var rejected = await client.PostAsync($"{yearUrl}/close", null))
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
        using (var rejected = await client.PostAsync($"{yearUrl}/soft-close", null))
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);

        using (var closed = await client.PostAsync($"{periodUrl}/close", null))
            Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode);
        using (var closedYear = await client.PostAsync($"{yearUrl}/close", null))
        {
            Assert.AreEqual(HttpStatusCode.OK, closedYear.StatusCode,
                await closedYear.Content.ReadAsStringAsync());
            Assert.AreEqual("Closed",
                (await closedYear.Content.ReadFromJsonAsync<FiscalYearResponse>())!.Status);
        }

        using (var rejected = await client.PostAsync($"{periodUrl}/reopen", null))
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);

        using (var reopened = await client.PostAsync($"{yearUrl}/reopen", null))
            Assert.AreEqual(HttpStatusCode.OK, reopened.StatusCode);
        using (var reopenedPeriod = await client.PostAsync($"{periodUrl}/reopen", null))
            Assert.AreEqual(HttpStatusCode.OK, reopenedPeriod.StatusCode);
    }

    [TestMethod]
    public async Task FiscalYearSoftCloseRequiresCompletedPeriodStatuses()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var yearUrl = $"/api/v1/accounting/fiscal-years/{calendar.YearId}";
        var periodUrl = $"{yearUrl}/periods/{calendar.PeriodId}";

        using (var softClosedPeriod = await client.PostAsync($"{periodUrl}/soft-close", null))
            Assert.AreEqual(HttpStatusCode.OK, softClosedPeriod.StatusCode);
        using (var softClosedYear = await client.PostAsync($"{yearUrl}/soft-close", null))
        {
            Assert.AreEqual(HttpStatusCode.OK, softClosedYear.StatusCode);
            Assert.AreEqual("SoftClosed",
                (await softClosedYear.Content.ReadFromJsonAsync<FiscalYearResponse>())!.Status);
        }

        using (var cannotHardClose = await client.PostAsync($"{yearUrl}/close", null))
            Assert.AreEqual(HttpStatusCode.Conflict, cannotHardClose.StatusCode);
        using (var reopenYear = await client.PostAsync($"{yearUrl}/reopen", null))
            Assert.AreEqual(HttpStatusCode.OK, reopenYear.StatusCode);
        using (var closePeriod = await client.PostAsync($"{periodUrl}/close", null))
            Assert.AreEqual(HttpStatusCode.OK, closePeriod.StatusCode);
        using (var closeYear = await client.PostAsync($"{yearUrl}/close", null))
            Assert.AreEqual(HttpStatusCode.OK, closeYear.StatusCode);
    }

    [TestMethod]
    public async Task FiscalCloseCannotModifyAnotherTenantCalendar()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var other = Client(userB, organizationB);
        using var request = await other.PostAsync(
            $"/api/v1/accounting/fiscal-years/{calendar.YearId}/periods/{calendar.PeriodId}/close",
            null);
        Assert.IsTrue(
            request.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.NotFound);

        await using var db = CreateDb();
        Assert.AreEqual(AccountingPeriodStatus.Open,
            (await db.AccountingPeriods.AsNoTracking()
                .SingleAsync(x => x.Id == calendar.PeriodId)).Status);
    }

    [TestMethod]
    public async Task ClosingAPeriodWaitsForExistingPostingTransaction()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);

        await using var locker = new NpgsqlConnection(connectionString);
        await locker.OpenAsync();
        await using var transaction = await locker.BeginTransactionAsync();
        await using (var blocking = new NpgsqlCommand(
            "SELECT id FROM accounting.fiscal_years WHERE organization_id = @org " +
            "AND id = @year FOR SHARE", locker, transaction))
        {
            blocking.Parameters.AddWithValue("org", organizationA);
            blocking.Parameters.AddWithValue("year", calendar.YearId);
            await using var reader = await blocking.ExecuteReaderAsync();
            Assert.IsTrue(await reader.ReadAsync());
        }

        var url = $"/api/v1/accounting/fiscal-years/{calendar.YearId}/periods/{calendar.PeriodId}/close";
        var closing = client.PostAsync(url, null);
        await Task.Delay(200);
        Assert.IsFalse(closing.IsCompleted, "Closing must wait for the in-flight posting lock.");

        await transaction.CommitAsync();
        using var response = await closing;
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task DraftHeaderAndLinesRejectEditsWhenPeriodIsClosed()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 80m, 80m);
        var firstLineId = draft.Lines[0].Id;

        // Simulate legacy/out-of-band closing of an existing draft's period.
        // The HTTP close route correctly rejects a period with drafts.
        await using (var db = CreateDb())
        {
            var period = await db.AccountingPeriods.SingleAsync(x =>
                x.Id == calendar.PeriodId);
            period.Close(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using (var header = await client.PutAsJsonAsync($"{Endpoint}/{draft.Id}",
            new UpdateJournalEntryRequest(PostingDate, PostingDate,
                "Modified despite close", null)))
            Assert.AreEqual(HttpStatusCode.Conflict, header.StatusCode);

        using (var add = await client.PostAsJsonAsync($"{Endpoint}/{draft.Id}/lines",
            new CreateJournalLineRequest(assetA, 15m, 0m, null)))
            Assert.AreEqual(HttpStatusCode.Conflict, add.StatusCode);

        using (var update = await client.PutAsJsonAsync(
            $"{Endpoint}/{draft.Id}/lines/{firstLineId}",
            new UpdateJournalLineRequest(assetA, 150m, 0m, null)))
            Assert.AreEqual(HttpStatusCode.Conflict, update.StatusCode);

        using (var delete = await client.DeleteAsync(
            $"{Endpoint}/{draft.Id}/lines/{firstLineId}"))
            Assert.AreEqual(HttpStatusCode.Conflict, delete.StatusCode);

        await using var verify = CreateDb();
        Assert.AreEqual(2, await verify.JournalLines.CountAsync(x =>
            x.JournalEntryId == draft.Id));
        Assert.AreEqual("HTTP financial regression",
            (await verify.JournalEntries.SingleAsync(x => x.Id == draft.Id)).Description);
    }

    [TestMethod]
    public async Task YearCloseCannotSkipStrandedDraftsInLegacyClosedPeriod()
    {
        var calendar = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        var draft = await CreateDraftAsync(client, 45m, 45m);

        // Legacy status can predate modern gates; a year finalization
        // must revalidate even if the period already says Closed.
        await using (var db = CreateDb())
        {
            var period = await db.AccountingPeriods.SingleAsync(x =>
                x.Id == calendar.PeriodId);
            period.Close(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using var close = await client.PostAsync(
            $"/api/v1/accounting/fiscal-years/{calendar.YearId}/close", null);
        Assert.AreEqual(HttpStatusCode.Conflict, close.StatusCode);

        await using var verify = CreateDb();
        Assert.AreEqual(FiscalYearStatus.Open,
            (await verify.FiscalYears.SingleAsync(x => x.Id == calendar.YearId)).Status);
        Assert.AreEqual(JournalEntryStatus.Draft,
            (await verify.JournalEntries.SingleAsync(x => x.Id == draft.Id)).Status);
    }

    private async Task<(Guid YearId, Guid PeriodId)> MarchCalendarAsync(Guid organizationId)
    {
        await using var db = CreateDb();
        var year = await db.FiscalYears.AsNoTracking()
            .SingleAsync(x => x.OrganizationId == organizationId);
        var period = await db.AccountingPeriods.AsNoTracking()
            .SingleAsync(x => x.OrganizationId == organizationId &&
                x.StartDate == new DateOnly(2026, 3, 1));
        return (year.Id, period.Id);
    }

    private ImportOpeningBalancesRequest ValidOpeningBalances(Guid fiscalYearId) =>
        new(fiscalYearId,
        [
            new CreateJournalLineRequest(assetA, 350m, 0m, null),
            new CreateJournalLineRequest(equityA, 0m, 350m, null)
        ]);

    private async Task<Guid> AddJanuaryOpeningPeriodAsync(Guid organizationId)
    {
        await using var db = CreateDb();
        var year = await db.FiscalYears.SingleAsync(x => x.OrganizationId == organizationId);
        db.AccountingPeriods.Add(AccountingPeriod.Create(
            organizationId,
            year.Id,
            1,
            "January opening",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31),
            DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        return year.Id;
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
