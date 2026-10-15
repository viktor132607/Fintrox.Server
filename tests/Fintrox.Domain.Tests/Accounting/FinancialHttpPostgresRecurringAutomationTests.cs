using System.Net;
using System.Net.Http.Json;
using Fintrox.Application.Accounting;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Real JWT HTTP + migrated PostgreSQL tests for opt-in recurring automation
/// and fenced multi-instance processing (no automatic GL posting).
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    private static string AutomationUrl(Guid template) =>
        $"/api/v1/accounting/recurring-journals/{template}/automation";

    private async Task<int> RunAutomationAsync(DateOnly date, int count = 10)
    {
        using var scope = factory!.Services.CreateScope();
        var automation = scope.ServiceProvider
            .GetRequiredService<IRecurringJournalAutomationService>();
        return await automation.ProcessDueAsync(date, count, CancellationToken.None);
    }

    [TestMethod]
    public async Task RecurringAutomationIsOptInAndCreatesOnlyOneUnpostedDraft()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        Assert.AreEqual(0, await RunAutomationAsync(PostingDate));

        using (var enable = await client.PutAsJsonAsync(
            AutomationUrl(template.Id),
            new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.OK, enable.StatusCode,
                await enable.Content.ReadAsStringAsync());

        Assert.AreEqual(1, await RunAutomationAsync(PostingDate));
        Assert.AreEqual(0, await RunAutomationAsync(PostingDate));

        using var response = await client.GetAsync(AutomationUrl(template.Id) + "/runs");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var runs = await response.Content
            .ReadFromJsonAsync<RecurringJournalAutomationRunResponse[]>();
        Assert.IsNotNull(runs);
        Assert.HasCount(1, runs);
        Assert.AreEqual("Generated", runs[0].Status);
        Assert.AreEqual(1, runs[0].AttemptCount);
        Assert.IsNotNull(runs[0].JournalEntryId);

        await using var db = CreateDb();
        var journal = await db.JournalEntries.AsNoTracking()
            .SingleAsync(x => x.Id == runs[0].JournalEntryId);
        Assert.AreEqual(JournalEntryStatus.Draft, journal.Status);
        Assert.IsNull(journal.Number);
        Assert.AreEqual(JournalEntrySource.System, journal.Source);
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task RecurringAutomationCanBeDisabledAndIsTenantIsolated()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var template = await CreateRecurringAsync(
            owner, RecurringExpenseRequest(assetA, expenseA));
        var url = AutomationUrl(template.Id);

        using (var denied = await other.PutAsJsonAsync(
            url, new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.NotFound, denied.StatusCode);
        using (var runsDenied = await other.GetAsync(url + "/runs"))
            Assert.AreEqual(HttpStatusCode.NotFound, runsDenied.StatusCode);

        using (var enabled = await owner.PutAsJsonAsync(
            url, new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.OK, enabled.StatusCode);
        using (var disabled = await owner.PutAsJsonAsync(
            url, new ConfigureRecurringJournalAutomationRequest(false)))
            Assert.AreEqual(HttpStatusCode.OK, disabled.StatusCode);
        Assert.AreEqual(0, await RunAutomationAsync(PostingDate));

        using var configuration = await owner.GetAsync(url);
        var state = await configuration.Content
            .ReadFromJsonAsync<RecurringJournalAutomationResponse>();
        Assert.IsNotNull(state);
        Assert.IsFalse(state.Enabled);
        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task ConcurrentRecurringAutomationWorkersClaimSingleRun()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using (var enable = await client.PutAsJsonAsync(
            AutomationUrl(template.Id),
            new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.OK, enable.StatusCode);

        var results = await Task.WhenAll(
            RunAutomationAsync(PostingDate, 1),
            RunAutomationAsync(PostingDate, 1));
        Assert.AreEqual(1, results.Sum());

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
        await using var cn = new NpgsqlConnection(connectionString);
        await cn.OpenAsync();
        await using var cmd = new NpgsqlCommand("""
            SELECT status,attempt_count FROM accounting.recurring_journal_automation_runs
            WHERE organization_id=@org AND template_id=@template
            """, cn);
        cmd.Parameters.AddWithValue("org", organizationA);
        cmd.Parameters.AddWithValue("template", template.Id);
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual("Generated", reader.GetString(0));
        Assert.AreEqual(1, reader.GetInt32(1));
    }

    [TestMethod]
    public async Task RecurringAutomationReclaimsExpiredLeaseWithoutDuplicatePosting()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using (var enable = await client.PutAsJsonAsync(
            AutomationUrl(template.Id),
            new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.OK, enable.StatusCode);

        await using (var cn = new NpgsqlConnection(connectionString))
        {
            await cn.OpenAsync();
            await using var seed = new NpgsqlCommand("""
                INSERT INTO accounting.recurring_journal_automation_runs
                    (id,organization_id,template_id,posting_date,status,
                     attempt_count,lease_token,lease_until_utc,next_attempt_at_utc)
                VALUES (@id,@org,@template,@date,'Claimed',1,@token,@expired,@expired)
                """, cn);
            seed.Parameters.AddWithValue("id", Guid.NewGuid());
            seed.Parameters.AddWithValue("org", organizationA);
            seed.Parameters.AddWithValue("template", template.Id);
            seed.Parameters.AddWithValue("date", PostingDate);
            seed.Parameters.AddWithValue("token", Guid.NewGuid());
            seed.Parameters.AddWithValue("expired", DateTimeOffset.UtcNow.AddMinutes(-10));
            await seed.ExecuteNonQueryAsync();
        }

        Assert.AreEqual(1, await RunAutomationAsync(PostingDate));
        using var response = await client.GetAsync(AutomationUrl(template.Id) + "/runs");
        var runs = await response.Content
            .ReadFromJsonAsync<RecurringJournalAutomationRunResponse[]>();
        Assert.IsNotNull(runs);
        Assert.HasCount(1, runs);
        Assert.AreEqual(2, runs[0].AttemptCount);
        Assert.AreEqual("Generated", runs[0].Status);
        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task RecurringAutomationRetriesClosedPeriodWithoutPosting()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using (var enable = await client.PutAsJsonAsync(
            AutomationUrl(template.Id),
            new ConfigureRecurringJournalAutomationRequest(true)))
            Assert.AreEqual(HttpStatusCode.OK, enable.StatusCode);

        var cal = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(client, cal.YearId, cal.PeriodId);
        Assert.AreEqual(1, await RunAutomationAsync(PostingDate));

        using var response = await client.GetAsync(AutomationUrl(template.Id) + "/runs");
        var runs = await response.Content
            .ReadFromJsonAsync<RecurringJournalAutomationRunResponse[]>();
        Assert.IsNotNull(runs);
        Assert.HasCount(1, runs);
        Assert.AreEqual("Retry", runs[0].Status);
        Assert.AreEqual(1, runs[0].AttemptCount);
        Assert.IsNull(runs[0].JournalEntryId);
        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }
}
