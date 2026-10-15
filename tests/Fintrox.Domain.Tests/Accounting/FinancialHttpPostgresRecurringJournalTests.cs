using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Real authenticated API + isolated, migrated PostgreSQL regression tests
/// for recurring journal templates and one-per-month generated drafts.
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    private const string RecurringUrl = "/api/v1/accounting/recurring-journals";

    private static CreateRecurringJournalTemplateRequest RecurringExpenseRequest(
        Guid bankAccount, Guid expenseAccount) =>
        new("Monthly services", "Monthly recurring general ledger expense",
            new DateOnly(2026, 3, 15), null,
            [
                new CreateJournalLineRequest(expenseAccount, 15m, 0m, "Monthly services expense"),
                new CreateJournalLineRequest(bankAccount, 0m, 15m, "Monthly services payment")
            ]);

    private static async Task<RecurringJournalTemplateResponse> CreateRecurringAsync(
        HttpClient client, CreateRecurringJournalTemplateRequest request)
    {
        using var created = await client.PostAsJsonAsync(RecurringUrl, request);
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode,
            await created.Content.ReadAsStringAsync());
        var template = await created.Content
            .ReadFromJsonAsync<RecurringJournalTemplateResponse>();
        Assert.IsNotNull(template);
        return template;
    }

    [TestMethod]
    public async Task RecurringJournalGeneratesUnpostedBalancedSystemDraftAndPostsViaGlRoute()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        Assert.AreEqual(15, template.DayOfMonth);
        Assert.AreEqual(2, template.Lines.Count);

        using var generatedResponse = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.OK, generatedResponse.StatusCode,
            await generatedResponse.Content.ReadAsStringAsync());
        var generated = await generatedResponse.Content
            .ReadFromJsonAsync<RecurringJournalGenerationResponse>();
        Assert.IsNotNull(generated);
        Assert.IsFalse(generated.AlreadyGenerated);
        Assert.AreEqual("Draft", generated.JournalStatus);

        await using (var db = CreateDb())
        {
            var journal = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == generated.JournalEntryId);
            Assert.AreEqual(JournalEntrySource.System, journal.Source);
            Assert.AreEqual(JournalEntryStatus.Draft, journal.Status);
            Assert.AreEqual(PostingDate, journal.PostingDate);
            Assert.AreEqual(
                $"recurring-journal:v1:{template.Id:N}:{PostingDate:yyyyMMdd}",
                journal.ExternalReference);
            var lines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == journal.Id)
                .OrderBy(x => x.LineNumber).ToArrayAsync();
            Assert.HasCount(2, lines);
            Assert.AreEqual(15m, lines.Sum(x => x.Debit));
            Assert.AreEqual(15m, lines.Sum(x => x.Credit));
            Assert.IsNull(journal.Number);
        }

        using (var tamper = await client.PostAsJsonAsync(
            $"/api/v1/accounting/journal/{generated.JournalEntryId}/lines",
            new CreateJournalLineRequest(expenseA, 1m, 0m, "Illegal edit")))
            Assert.AreEqual(HttpStatusCode.Conflict, tamper.StatusCode);

        using var postedResponse = await client.PostAsync(
            $"/api/v1/accounting/journal/{generated.JournalEntryId}/post", null);
        Assert.AreEqual(HttpStatusCode.OK, postedResponse.StatusCode,
            await postedResponse.Content.ReadAsStringAsync());
        var posted = await postedResponse.Content
            .ReadFromJsonAsync<JournalEntryResponse>();
        Assert.IsNotNull(posted);
        Assert.AreEqual("Posted", posted.Status);
        Assert.IsNotNull(posted.Number);

        using var replay = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var again = await replay.Content
            .ReadFromJsonAsync<RecurringJournalGenerationResponse>();
        Assert.IsNotNull(again);
        Assert.IsTrue(again.AlreadyGenerated);
        Assert.AreEqual("Posted", again.JournalStatus);
        Assert.AreEqual(generated.JournalEntryId, again.JournalEntryId);
    }

    [TestMethod]
    public async Task RecurringJournalRejectsUnbalancedAndCrossTenantTemplateAccounts()
    {
        using var client = Client(userA, organizationA);
        var request = RecurringExpenseRequest(assetA, expenseA);
        using (var notBalanced = await client.PostAsJsonAsync(
            RecurringUrl, request with
            {
                Lines =
                [
                    new CreateJournalLineRequest(expenseA, 20m, 0m, null),
                    new CreateJournalLineRequest(assetA, 0m, 15m, null)
                ]
            }))
            Assert.AreEqual(HttpStatusCode.Conflict, notBalanced.StatusCode);

        using (var crossTenant = await client.PostAsJsonAsync(
            RecurringUrl, RecurringExpenseRequest(assetB, expenseA)))
            Assert.AreEqual(HttpStatusCode.Conflict, crossTenant.StatusCode);

        using (var wrongDay = await client.PostAsJsonAsync(
            RecurringUrl, request with
            {
                FirstPostingDate = new DateOnly(2026, 3, 30)
            }))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongDay.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task RecurringJournalGenerationChecksScheduleAndClosedPeriod()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));

        using (var mismatch = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(new DateOnly(2026, 3, 16))))
            Assert.AreEqual(HttpStatusCode.Conflict, mismatch.StatusCode);

        var cal = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(client, cal.YearId, cal.PeriodId);
        using var denied = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.Conflict, denied.StatusCode);
        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task RecurringJournalGenerationIsTenantScoped()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var template = await CreateRecurringAsync(
            owner, RecurringExpenseRequest(assetA, expenseA));

        using var rejected = await other.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.NotFound, rejected.StatusCode);

        using var listResponse = await other.GetAsync(RecurringUrl);
        Assert.AreEqual(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content
            .ReadFromJsonAsync<RecurringJournalTemplateResponse[]>();
        Assert.IsNotNull(list);
        Assert.AreEqual(0, list.Length);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationB));
    }

    [TestMethod]
    public async Task ConcurrentRecurringJournalGenerationCreatesOnlyOneDraft()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            first, RecurringExpenseRequest(assetA, expenseA));
        var url = $"{RecurringUrl}/{template.Id}/generate";
        var request = new GenerateRecurringJournalRequest(PostingDate);
        var responses = await Task.WhenAll(
            first.PostAsJsonAsync(url, request),
            second.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(responses.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join("; ", await Task.WhenAll(responses.Select(x =>
                    x.Content.ReadAsStringAsync()))));
            var results = await Task.WhenAll(responses.Select(x =>
                x.Content.ReadFromJsonAsync<RecurringJournalGenerationResponse>()));
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[1]);
            Assert.AreEqual(results[0]!.JournalEntryId,
                results[1]!.JournalEntryId);
            Assert.AreNotEqual(results[0]!.AlreadyGenerated,
                results[1]!.AlreadyGenerated);
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA &&
            x.ExternalReference ==
                $"recurring-journal:v1:{template.Id:N}:{PostingDate:yyyyMMdd}"));
    }
}
