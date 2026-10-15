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

    [TestMethod]
    public async Task RecurringScheduleReportsOverdueScheduledDraftAndPostedWithoutSideEffects()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        var url = $"{RecurringUrl}/{template.Id}/schedule?from=2026-03-01&to=2026-06-30&asOf=2026-04-01";

        using (var before = await client.GetAsync(url))
        {
            Assert.AreEqual(HttpStatusCode.OK, before.StatusCode);
            var schedule = await before.Content
                .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(new DateOnly(2026, 4, 1), schedule.AsOfDate);
            Assert.HasCount(4, schedule.Occurrences);
            Assert.AreEqual("Overdue", schedule.Occurrences[0].OccurrenceStatus);
            Assert.AreEqual("Scheduled", schedule.Occurrences[1].OccurrenceStatus);
            Assert.IsNull(schedule.Occurrences[0].JournalEntryId);
        }
        await using (var db = CreateDb())
            Assert.AreEqual(0, await db.JournalEntries.CountAsync(
                x => x.OrganizationId == organizationA));

        using var gen = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{template.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.OK, gen.StatusCode,
            await gen.Content.ReadAsStringAsync());
        var generated = await gen.Content
            .ReadFromJsonAsync<RecurringJournalGenerationResponse>();
        Assert.IsNotNull(generated);

        using (var draftResponse = await client.GetAsync(url))
        {
            var draft = await draftResponse.Content
                .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
            Assert.IsNotNull(draft);
            Assert.AreEqual("Draft", draft.Occurrences[0].OccurrenceStatus);
            Assert.AreEqual(generated.JournalEntryId,
                draft.Occurrences[0].JournalEntryId);
        }

        using (var post = await client.PostAsync(
            $"/api/v1/accounting/journal/{generated.JournalEntryId}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, post.StatusCode,
                await post.Content.ReadAsStringAsync());

        using var postedResponse = await client.GetAsync(url);
        var posted = await postedResponse.Content
            .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
        Assert.IsNotNull(posted);
        Assert.AreEqual("Posted", posted.Occurrences[0].OccurrenceStatus);
        Assert.AreEqual("Scheduled", posted.Occurrences[1].OccurrenceStatus);
    }

    [TestMethod]
    public async Task RecurringScheduleRespectsCutoffTodayAndTenantBoundary()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var request = RecurringExpenseRequest(assetA, expenseA) with
        {
            LastPostingDate = new DateOnly(2026, 5, 15)
        };
        var template = await CreateRecurringAsync(owner, request);
        var url = $"{RecurringUrl}/{template.Id}/schedule?from=2026-04-01&to=2026-07-01&asOf=2026-04-15";

        using (var otherTenant = await other.GetAsync(url))
            Assert.AreEqual(HttpStatusCode.NotFound, otherTenant.StatusCode);

        using var response = await owner.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var schedule = await response.Content
            .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
        Assert.IsNotNull(schedule);
        Assert.HasCount(2, schedule.Occurrences);
        Assert.AreEqual(new DateOnly(2026, 4, 15),
            schedule.Occurrences[0].PostingDate);
        Assert.AreEqual("Due", schedule.Occurrences[0].OccurrenceStatus);
        Assert.AreEqual(new DateOnly(2026, 5, 15),
            schedule.Occurrences[1].PostingDate);
        Assert.AreEqual("Scheduled", schedule.Occurrences[1].OccurrenceStatus);
    }

    [TestMethod]
    public async Task RecurringScheduleRejectsUnboundedAndReversedDates()
    {
        using var client = Client(userA, organizationA);
        var template = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using (var tooLong = await client.GetAsync(
            $"{RecurringUrl}/{template.Id}/schedule?from=2026-01-01&to=2028-01-01"))
            Assert.AreEqual(HttpStatusCode.Conflict, tooLong.StatusCode);
        using (var reversed = await client.GetAsync(
            $"{RecurringUrl}/{template.Id}/schedule?from=2026-06-01&to=2026-05-01"))
            Assert.AreEqual(HttpStatusCode.Conflict, reversed.StatusCode);
        using var absent = await client.GetAsync(
            $"{RecurringUrl}/{Guid.NewGuid()}/schedule?from=2026-03-01&to=2026-05-01");
        Assert.AreEqual(HttpStatusCode.NotFound, absent.StatusCode);
    }


    [TestMethod]
    public async Task RecurringRevisionKeepsPriorHistoryAndCutsOverSchedules()
    {
        using var client = Client(userA, organizationA);
        var original = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using (var generated = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate)))
            Assert.AreEqual(HttpStatusCode.OK, generated.StatusCode);

        var replacement = RecurringExpenseRequest(assetA, expenseA) with
        {
            Name = "Revised monthly services",
            FirstPostingDate = new DateOnly(2026, 4, 20),
            Lines =
            [
                new CreateJournalLineRequest(expenseA, 25m, 0m, "New service expense"),
                new CreateJournalLineRequest(assetA, 0m, 25m, "New service payment")
            ]
        };
        using var revised = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 4, 1), replacement));
        Assert.AreEqual(HttpStatusCode.Created, revised.StatusCode,
            await revised.Content.ReadAsStringAsync());
        var result = await revised.Content
            .ReadFromJsonAsync<RecurringJournalRevisionResponse>();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Version);
        Assert.AreEqual(original.Id, result.RootTemplateId);
        Assert.AreEqual(25m, result.Replacement.Lines.Sum(x => x.Debit));

        using var versionsResult = await client.GetAsync(
            $"{RecurringUrl}/{result.Replacement.Id}/versions");
        Assert.AreEqual(HttpStatusCode.OK, versionsResult.StatusCode);
        var lineage = await versionsResult.Content
            .ReadFromJsonAsync<RecurringJournalLineageResponse>();
        Assert.IsNotNull(lineage);
        Assert.HasCount(2, lineage.Versions);
        Assert.AreEqual(original.Id, lineage.Versions[0].TemplateId);
        Assert.AreEqual(new DateOnly(2026, 4, 1),
            lineage.Versions[0].SupersededFromMonth);
        Assert.AreEqual(original.Id, lineage.Versions[1].PreviousTemplateId);

        using var oldScheduleResponse = await client.GetAsync(
            $"{RecurringUrl}/{original.Id}/schedule?from=2026-03-01&to=2026-06-30&asOf=2026-04-01");
        var oldSchedule = await oldScheduleResponse.Content
            .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
        Assert.IsNotNull(oldSchedule);
        Assert.HasCount(1, oldSchedule.Occurrences);
        Assert.AreEqual("Draft", oldSchedule.Occurrences[0].OccurrenceStatus);

        using var newScheduleResponse = await client.GetAsync(
            $"{RecurringUrl}/{result.Replacement.Id}/schedule?from=2026-03-01&to=2026-06-30&asOf=2026-04-01");
        var newSchedule = await newScheduleResponse.Content
            .ReadFromJsonAsync<RecurringJournalScheduleResponse>();
        Assert.IsNotNull(newSchedule);
        Assert.HasCount(3, newSchedule.Occurrences);
        Assert.AreEqual(new DateOnly(2026, 4, 20),
            newSchedule.Occurrences[0].PostingDate);

        using (var stale = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/generate",
            new GenerateRecurringJournalRequest(new DateOnly(2026, 4, 15))))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, stale.StatusCode);
            StringAssert.Contains(await stale.Content.ReadAsStringAsync(),
                "superseded");
        }

        using var marchReplay = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate));
        Assert.AreEqual(HttpStatusCode.OK, marchReplay.StatusCode);
        var replay = await marchReplay.Content
            .ReadFromJsonAsync<RecurringJournalGenerationResponse>();
        Assert.IsNotNull(replay);
        Assert.IsTrue(replay.AlreadyGenerated);
    }

    [TestMethod]
    public async Task RecurringRevisionSupportsChainedVersionsButNotBranching()
    {
        using var client = Client(userA, organizationA);
        var original = await CreateRecurringAsync(
            client, RecurringExpenseRequest(assetA, expenseA));
        using var v2Response = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 4, 1),
                RecurringExpenseRequest(assetA, expenseA) with
                {
                    FirstPostingDate = new DateOnly(2026, 4, 20)
                }));
        Assert.AreEqual(HttpStatusCode.Created, v2Response.StatusCode);
        var v2 = await v2Response.Content
            .ReadFromJsonAsync<RecurringJournalRevisionResponse>();
        Assert.IsNotNull(v2);

        using (var branch = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 5, 1),
                RecurringExpenseRequest(assetA, expenseA) with
                {
                    FirstPostingDate = new DateOnly(2026, 5, 15)
                })))
            Assert.AreEqual(HttpStatusCode.Conflict, branch.StatusCode);

        using var v3Response = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{v2.Replacement.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 6, 1),
                RecurringExpenseRequest(assetA, expenseA) with
                {
                    FirstPostingDate = new DateOnly(2026, 6, 23)
                }));
        Assert.AreEqual(HttpStatusCode.Created, v3Response.StatusCode,
            await v3Response.Content.ReadAsStringAsync());
        var v3 = await v3Response.Content
            .ReadFromJsonAsync<RecurringJournalRevisionResponse>();
        Assert.IsNotNull(v3);
        Assert.AreEqual(3, v3.Version);
        Assert.AreEqual(original.Id, v3.RootTemplateId);

        using var versionsResponse = await client.GetAsync(
            $"{RecurringUrl}/{original.Id}/versions");
        var versions = await versionsResponse.Content
            .ReadFromJsonAsync<RecurringJournalLineageResponse>();
        Assert.IsNotNull(versions);
        Assert.HasCount(3, versions.Versions);
        Assert.AreEqual(new DateOnly(2026, 6, 1),
            versions.Versions[1].SupersededFromMonth);
        Assert.IsNull(versions.Versions[2].SupersededFromMonth);
    }

    [TestMethod]
    public async Task RecurringRevisionRejectsExistingGeneratedCutoverAndInvalidRequest()
    {
        using var client = Client(userA, organizationA);
        var initial = RecurringExpenseRequest(assetA, expenseA) with
        {
            FirstPostingDate = new DateOnly(2026, 1, 15)
        };
        var original = await CreateRecurringAsync(client, initial);
        using (var generated = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/generate",
            new GenerateRecurringJournalRequest(PostingDate)))
            Assert.AreEqual(HttpStatusCode.OK, generated.StatusCode);

        using (var alreadyGenerated = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 3, 1),
                initial with { FirstPostingDate = new DateOnly(2026, 3, 20) })))
            Assert.AreEqual(HttpStatusCode.Conflict, alreadyGenerated.StatusCode);

        using (var notFirst = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 4, 2),
                initial with { FirstPostingDate = new DateOnly(2026, 4, 20) })))
            Assert.AreEqual(HttpStatusCode.Conflict, notFirst.StatusCode);

        using (var invalidLines = await client.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            new ReviseRecurringJournalTemplateRequest(
                new DateOnly(2026, 4, 1),
                initial with
                {
                    FirstPostingDate = new DateOnly(2026, 4, 15),
                    Lines =
                    [
                        new CreateJournalLineRequest(expenseA, 20m, 0m, null),
                        new CreateJournalLineRequest(assetA, 0m, 15m, null)
                    ]
                })))
            Assert.AreEqual(HttpStatusCode.Conflict, invalidLines.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(
            x => x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task RecurringRevisionIsTenantScoped()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var original = await CreateRecurringAsync(
            owner, RecurringExpenseRequest(assetA, expenseA));
        var request = new ReviseRecurringJournalTemplateRequest(
            new DateOnly(2026, 4, 1),
            RecurringExpenseRequest(assetA, expenseA) with
            {
                FirstPostingDate = new DateOnly(2026, 4, 15)
            });
        using (var denied = await other.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise", request))
            Assert.AreEqual(HttpStatusCode.NotFound, denied.StatusCode);

        using var lineage = await other.GetAsync(
            $"{RecurringUrl}/{original.Id}/versions");
        Assert.AreEqual(HttpStatusCode.NotFound, lineage.StatusCode);

        using (var crossTenant = await owner.PostAsJsonAsync(
            $"{RecurringUrl}/{original.Id}/revise",
            request with
            {
                Replacement = request.Replacement with
                {
                    Lines =
                    [
                        new CreateJournalLineRequest(expenseA, 15m, 0m, null),
                        new CreateJournalLineRequest(assetB, 0m, 15m, null)
                    ]
                }
            }))
            Assert.AreEqual(HttpStatusCode.Conflict, crossTenant.StatusCode);
    }

    [TestMethod]
    public async Task ConcurrentRecurringRevisionAllowsExactlyOneSuccessor()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var original = await CreateRecurringAsync(
            first, RecurringExpenseRequest(assetA, expenseA));
        var request = new ReviseRecurringJournalTemplateRequest(
            new DateOnly(2026, 4, 1),
            RecurringExpenseRequest(assetA, expenseA) with
            {
                FirstPostingDate = new DateOnly(2026, 4, 20)
            });
        var url = $"{RecurringUrl}/{original.Id}/revise";
        var responses = await Task.WhenAll(
            first.PostAsJsonAsync(url, request),
            second.PostAsJsonAsync(url, request));
        try
        {
            Assert.AreEqual(1, responses.Count(x =>
                x.StatusCode == HttpStatusCode.Created),
                string.Join(" | ", await Task.WhenAll(
                    responses.Select(x => x.Content.ReadAsStringAsync()))));
            Assert.AreEqual(1, responses.Count(x =>
                x.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
        using var lineageResponse = await first.GetAsync(
            $"{RecurringUrl}/{original.Id}/versions");
        var lineage = await lineageResponse.Content
            .ReadFromJsonAsync<RecurringJournalLineageResponse>();
        Assert.IsNotNull(lineage);
        Assert.HasCount(2, lineage.Versions);
    }

}
