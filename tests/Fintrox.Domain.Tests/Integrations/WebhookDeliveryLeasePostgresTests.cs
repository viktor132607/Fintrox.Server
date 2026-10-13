using Fintrox.Domain.Integrations;
using Fintrox.Domain.Organizations;
using Fintrox.Infrastructure.Integrations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Integrations;

/// <summary>
/// Exercises competing repository instances against real migrated PostgreSQL,
/// including conditional claims, crashed-worker lease expiry and stale writes.
/// </summary>
[TestClass]
public sealed class WebhookDeliveryLeasePostgresTests
{
    private string? adminConnection;
    private string? databaseName;
    private string connectionString = null!;
    private Guid deliveryId;
    private Guid organizationId;
    private readonly TimeSpan leaseDuration = TimeSpan.FromMinutes(2);
    private readonly DateTimeOffset now = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("FINTROX_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            Assert.Inconclusive(
                "Set FINTROX_TEST_POSTGRES to a disposable PostgreSQL server with CREATE DATABASE permission.");
        }

        databaseName = "fintrox_delivery_lease_" + Guid.NewGuid().ToString("N");
        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        connectionString = new NpgsqlConnectionStringBuilder(adminConnection)
        {
            Database = databaseName, Pooling = false
        }.ConnectionString;

        await using var db = CreateDb();
        await db.Database.MigrateAsync();
        var org = Organization.Create(
            "Test organization", null, "webhook-lease-test",
            "BG", "EUR", "Europe/Sofia", null, null, now);
        organizationId = org.Id;
        db.Organizations.Add(org);
        var subscription = WebhookSubscription.Create(
            organizationId, "Integration", "https://hooks.example.com/receive",
            "*", "ciphertext", "whsec_prefix", now);
        var integrationEvent = IntegrationEvent.Create(
            organizationId, null, "order.created", "Order", Guid.NewGuid(),
            "{\"amount\":10}", now, now);
        var delivery = WebhookDelivery.Create(
            organizationId, integrationEvent.Id, subscription.Id, now);
        deliveryId = delivery.Id;
        db.WebhookSubscriptions.Add(subscription);
        db.IntegrationEvents.Add(integrationEvent);
        db.WebhookDeliveries.Add(delivery);
        await db.SaveChangesAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (databaseName is null || adminConnection is null)
        {
            return;
        }

        await using var admin = new NpgsqlConnection(adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }

    [TestMethod]
    public async Task TwoWorkersCanClaimEachDueDeliveryOnlyOnce()
    {
        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var workerA = ClaimAfterSignalAsync(start.Task);
        var workerB = ClaimAfterSignalAsync(start.Task);
        start.SetResult();

        var claims = await Task.WhenAll(workerA, workerB);
        Assert.AreEqual(1, claims.Count(claim => claim is not null));
        Assert.IsFalse(claims.Single(claim => claim is not null)!.IsExhausted);

        await using var verify = CreateDb();
        var record = await verify.WebhookDeliveries.AsNoTracking().SingleAsync();
        Assert.AreEqual(deliveryId, record.Id);
        Assert.AreEqual(1, record.AttemptCount);
        Assert.AreEqual(WebhookDeliveryStatus.Pending, record.Status);
        Assert.AreEqual(now + leaseDuration, record.NextAttemptAtUtc);
    }

    [TestMethod]
    public async Task ExpiredLeaseIsReclaimedButStaleWorkerCannotMarkDelivered()
    {
        await using var workerA = CreateDb();
        var repoA = new IntegrationWorkflowRepository(workerA);
        var first = await repoA.ClaimNextDueDeliveryAsync(
            now, leaseDuration, 6, CancellationToken.None);
        Assert.IsNotNull(first);

        await using var workerB = CreateDb();
        var repoB = new IntegrationWorkflowRepository(workerB);
        Assert.IsNull(await repoB.ClaimNextDueDeliveryAsync(
            now.Add(leaseDuration).AddMilliseconds(-1), leaseDuration, 6, CancellationToken.None));

        var second = await repoB.ClaimNextDueDeliveryAsync(
            now.Add(leaseDuration).AddSeconds(1), leaseDuration, 6, CancellationToken.None);
        Assert.IsNotNull(second);
        Assert.AreEqual(2, second.Delivery.AttemptCount);

        first.Delivery.Succeed(200, now.AddMinutes(2).AddSeconds(2));
        Assert.IsFalse(await repoA.TryPersistClaimedDeliveryAsync(
            first.Delivery, first.Delivery.NextAttemptAtUtc, CancellationToken.None));

        var currentLeaseDeadline = second.Delivery.NextAttemptAtUtc;
        second.Delivery.Succeed(202, now.AddMinutes(2).AddSeconds(3));
        Assert.IsTrue(await repoB.TryPersistClaimedDeliveryAsync(
            second.Delivery, currentLeaseDeadline, CancellationToken.None));

        await using var verify = CreateDb();
        var stored = await verify.WebhookDeliveries.AsNoTracking().SingleAsync();
        Assert.AreEqual(2, stored.AttemptCount);
        Assert.AreEqual(202, stored.LastStatusCode);
        Assert.AreEqual(WebhookDeliveryStatus.Delivered, stored.Status);
    }

    [TestMethod]
    public async Task RetryReleasesClaimOnlyAtNextAttemptDate()
    {
        await using var worker = CreateDb();
        var repo = new IntegrationWorkflowRepository(worker);
        var first = await repo.ClaimNextDueDeliveryAsync(
            now, leaseDuration, 6, CancellationToken.None);
        Assert.IsNotNull(first);
        var deadline = first.Delivery.NextAttemptAtUtc;
        first.Delivery.ScheduleRetry(503, "Retry later", now.AddMinutes(5), now.AddSeconds(1));
        Assert.IsTrue(await repo.TryPersistClaimedDeliveryAsync(
            first.Delivery, deadline, CancellationToken.None));

        await using var contender = CreateDb();
        var other = new IntegrationWorkflowRepository(contender);
        Assert.IsNull(await other.ClaimNextDueDeliveryAsync(
            now.AddMinutes(4), leaseDuration, 6, CancellationToken.None));

        var second = await other.ClaimNextDueDeliveryAsync(
            now.AddMinutes(5), leaseDuration, 6, CancellationToken.None);
        Assert.IsNotNull(second);
        Assert.AreEqual(2, second.Delivery.AttemptCount);
        Assert.IsFalse(second.IsExhausted);
    }

    [TestMethod]
    public async Task ExpiredSixthLeaseIsTerminalWithoutASeventhHttpAttempt()
    {
        await using var worker = CreateDb();
        var repo = new IntegrationWorkflowRepository(worker);
        for (var index = 0; index < 6; index++)
        {
            var at = now.AddMinutes(index * 5);
            var claimed = await repo.ClaimNextDueDeliveryAsync(
                at, leaseDuration, 6, CancellationToken.None);
            Assert.IsNotNull(claimed);
            Assert.IsFalse(claimed.IsExhausted);
            Assert.AreEqual(index + 1, claimed.Delivery.AttemptCount);

            if (index < 5)
            {
                var deadline = claimed.Delivery.NextAttemptAtUtc;
                claimed.Delivery.ScheduleRetry(
                    503, "Unavailable", at.AddMinutes(5), at.AddSeconds(1));
                Assert.IsTrue(await repo.TryPersistClaimedDeliveryAsync(
                    claimed.Delivery, deadline, CancellationToken.None));
            }
        }

        await using var recoveryWorker = CreateDb();
        var recovery = new IntegrationWorkflowRepository(recoveryWorker);
        var reclaimed = await recovery.ClaimNextDueDeliveryAsync(
            now.AddMinutes(27), leaseDuration, 6, CancellationToken.None);
        Assert.IsNotNull(reclaimed);
        Assert.IsTrue(reclaimed.IsExhausted);
        Assert.AreEqual(6, reclaimed.Delivery.AttemptCount);

        var deadlineOnRecovery = reclaimed.Delivery.NextAttemptAtUtc;
        reclaimed.Delivery.Fail(null, "Attempt limit reached", now.AddMinutes(27));
        Assert.IsTrue(await recovery.TryPersistClaimedDeliveryAsync(
            reclaimed.Delivery, deadlineOnRecovery, CancellationToken.None));
        Assert.IsNull(await repo.ClaimNextDueDeliveryAsync(
            now.AddDays(1), leaseDuration, 6, CancellationToken.None));

        await using var verify = CreateDb();
        var saved = await verify.WebhookDeliveries.AsNoTracking().SingleAsync();
        Assert.AreEqual(6, saved.AttemptCount);
        Assert.AreEqual(WebhookDeliveryStatus.Failed, saved.Status);
    }

    private async Task<Fintrox.Application.Integrations.WebhookDeliveryClaim?> ClaimAfterSignalAsync(
        Task start)
    {
        await start;
        await using var db = CreateDb();
        return await new IntegrationWorkflowRepository(db).ClaimNextDueDeliveryAsync(
            now, leaseDuration, 6, CancellationToken.None);
    }

    private FintroxDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FintroxDbContext>()
            .UseNpgsql(connectionString).Options;
        return new FintroxDbContext(options);
    }
}
