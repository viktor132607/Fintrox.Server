using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Migrated PostgreSQL + authenticated HTTP regression tests for immutable
/// accrual/deferral schedules and separately postable monthly system drafts.
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    private const string RecognitionUrl = "/api/v1/accounting/recognition-schedules";

    private CreateRecognitionScheduleRequest PrepaidRequest() =>
        new("PrepaidExpense", "Insurance premium",
            "Monthly straight-line expense recognition for a prepaid balance",
            PostingDate, 3, 10m, expenseA, assetA);

    private static async Task<RecognitionScheduleResponse> CreateRecognitionAsync(
        HttpClient client, CreateRecognitionScheduleRequest request)
    {
        using var response = await client.PostAsJsonAsync(RecognitionUrl, request);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var schedule = await response.Content.ReadFromJsonAsync<RecognitionScheduleResponse>();
        Assert.IsNotNull(schedule);
        return schedule;
    }

    [TestMethod]
    public async Task RecognitionScheduleSplitsRoundingResidualAndGeneratesUnpostedImmutableDraft()
    {
        using var client = Client(userA, organizationA);
        var schedule = await CreateRecognitionAsync(client, PrepaidRequest());
        Assert.AreEqual(3.3333m, schedule.MonthlyAmount);

        using (var get = await client.GetAsync($"{RecognitionUrl}/{schedule.Id}"))
        {
            Assert.AreEqual(HttpStatusCode.OK, get.StatusCode);
            var detail = await get.Content.ReadFromJsonAsync<RecognitionScheduleDetailResponse>();
            Assert.IsNotNull(detail);
            Assert.HasCount(3, detail.Occurrences);
            Assert.AreEqual(3.3333m, detail.Occurrences[0].Amount);
            Assert.AreEqual(3.3333m, detail.Occurrences[1].Amount);
            Assert.AreEqual(3.3334m, detail.Occurrences[2].Amount);
            Assert.AreEqual(10m, detail.Occurrences.Sum(x => x.Amount));
            Assert.IsTrue(detail.Occurrences.All(x => x.JournalEntryId is null));
        }

        var url = $"{RecognitionUrl}/{schedule.Id}/generate";
        using var response = await client.PostAsJsonAsync(
            url, new GenerateRecognitionRequest(1));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var generated = await response.Content
            .ReadFromJsonAsync<RecognitionGenerationResponse>();
        Assert.IsNotNull(generated);
        Assert.IsFalse(generated.AlreadyGenerated);
        Assert.AreEqual("Draft", generated.JournalStatus);
        Assert.AreEqual(3.3333m, generated.Amount);
        Assert.AreEqual(PostingDate, generated.PostingDate);

        await using (var db = CreateDb())
        {
            var draft = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == generated.JournalEntryId);
            Assert.AreEqual(JournalEntrySource.System, draft.Source);
            Assert.AreEqual(JournalEntryStatus.Draft, draft.Status);
            Assert.IsNull(draft.Number);
            Assert.AreEqual(
                $"recognition-schedule:v1:{schedule.Id:N}:1",
                draft.ExternalReference);
            var lines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == draft.Id)
                .OrderBy(x => x.LineNumber).ToArrayAsync();
            Assert.HasCount(2, lines);
            Assert.AreEqual(expenseA, lines[0].AccountId);
            Assert.AreEqual(assetA, lines[1].AccountId);
            Assert.AreEqual(3.3333m, lines.Sum(x => x.Debit));
            Assert.AreEqual(3.3333m, lines.Sum(x => x.Credit));
        }

        using (var edit = await client.PostAsJsonAsync(
            $"/api/v1/accounting/journal/{generated.JournalEntryId}/lines",
            new CreateJournalLineRequest(expenseA, 1m, 0m, "Cannot edit source")))
            Assert.AreEqual(HttpStatusCode.Conflict, edit.StatusCode);

        using (var post = await client.PostAsync(
            $"/api/v1/accounting/journal/{generated.JournalEntryId}/post", null))
            Assert.AreEqual(HttpStatusCode.OK, post.StatusCode,
                await post.Content.ReadAsStringAsync());

        using var replay = await client.PostAsJsonAsync(
            url, new GenerateRecognitionRequest(1));
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var again = await replay.Content.ReadFromJsonAsync<RecognitionGenerationResponse>();
        Assert.IsNotNull(again);
        Assert.IsTrue(again.AlreadyGenerated);
        Assert.AreEqual(generated.JournalEntryId, again.JournalEntryId);
        Assert.AreEqual("Posted", again.JournalStatus);

        using var after = await client.GetAsync($"{RecognitionUrl}/{schedule.Id}");
        var detailAfter = await after.Content
            .ReadFromJsonAsync<RecognitionScheduleDetailResponse>();
        Assert.IsNotNull(detailAfter);
        Assert.AreEqual("Posted", detailAfter.Occurrences[0].Status);
        Assert.AreEqual(generated.JournalEntryId,
            detailAfter.Occurrences[0].JournalEntryId);
    }

    [TestMethod]
    public async Task RecognitionKindsRequireCorrectBalanceSheetAndProfitLossAccountTypes()
    {
        Guid liability;
        await using (var db = CreateDb())
        {
            var account = Account.Create(organizationA, "402", "Accrued liability",
                AccountType.Liability, null, true, true, DateTimeOffset.UtcNow);
            liability = account.Id;
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
        }

        using var client = Client(userA, organizationA);
        var prepaid = PrepaidRequest();
        var cases = new[]
        {
            prepaid,
            prepaid with { Kind = "DeferredRevenue", DebitAccountId = liability,
                CreditAccountId = revenueA },
            prepaid with { Kind = "AccruedExpense", DebitAccountId = expenseA,
                CreditAccountId = liability },
            prepaid with { Kind = "AccruedRevenue", DebitAccountId = assetA,
                CreditAccountId = revenueA }
        };

        foreach (var request in cases)
        {
            var created = await CreateRecognitionAsync(client, request);
            Assert.AreEqual(request.Kind, created.Kind);
            Assert.AreEqual(request.TotalAmount, created.TotalAmount);
        }

        using var incorrect = await client.PostAsJsonAsync(
            RecognitionUrl, prepaid with
            {
                Kind = "DeferredRevenue", DebitAccountId = expenseA,
                CreditAccountId = assetA
            });
        Assert.AreEqual(HttpStatusCode.Conflict, incorrect.StatusCode);
    }

    [TestMethod]
    public async Task RecognitionRejectsInvalidAmountsDatesCountsAndCrossTenantAccounts()
    {
        using var client = Client(userA, organizationA);
        var request = PrepaidRequest();
        var invalid = new[]
        {
            request with { Kind = "InvalidKind" },
            request with { Months = 0 },
            request with { Months = 121 },
            request with { TotalAmount = 0m },
            request with { TotalAmount = 0.0001m, Months = 2 },
            request with { TotalAmount = 10.00001m },
            request with { FirstPostingDate = new DateOnly(2026, 3, 29) },
            request with { CreditAccountId = assetB },
            request with { DebitAccountId = assetA }
        };

        foreach (var attempt in invalid)
        {
            using var rejected = await client.PostAsJsonAsync(RecognitionUrl, attempt);
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode,
                await rejected.Content.ReadAsStringAsync());
        }

        using var listed = await client.GetAsync(RecognitionUrl);
        Assert.AreEqual(HttpStatusCode.OK, listed.StatusCode);
        var schedules = await listed.Content
            .ReadFromJsonAsync<RecognitionScheduleResponse[]>();
        Assert.IsNotNull(schedules);
        Assert.HasCount(0, schedules);
    }

    [TestMethod]
    public async Task RecognitionGenerationChecksPeriodFiscalCloseAndTenantAccess()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var schedule = await CreateRecognitionAsync(owner, PrepaidRequest());

        using (var wrongPeriod = await owner.PostAsJsonAsync(
            $"{RecognitionUrl}/{schedule.Id}/generate",
            new GenerateRecognitionRequest(4)))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongPeriod.StatusCode);

        using (var deniedGet = await other.GetAsync($"{RecognitionUrl}/{schedule.Id}"))
            Assert.AreEqual(HttpStatusCode.NotFound, deniedGet.StatusCode);
        using (var deniedGenerate = await other.PostAsJsonAsync(
            $"{RecognitionUrl}/{schedule.Id}/generate",
            new GenerateRecognitionRequest(1)))
            Assert.AreEqual(HttpStatusCode.NotFound, deniedGenerate.StatusCode);

        var cal = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(owner, cal.YearId, cal.PeriodId);
        using var closed = await owner.PostAsJsonAsync(
            $"{RecognitionUrl}/{schedule.Id}/generate",
            new GenerateRecognitionRequest(1));
        Assert.AreEqual(HttpStatusCode.Conflict, closed.StatusCode,
            await closed.Content.ReadAsStringAsync());

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(
            x => x.OrganizationId == organizationA));
    }

    [TestMethod]
    public async Task ConcurrentRecognitionGenerationIsIdempotent()
    {
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var schedule = await CreateRecognitionAsync(first, PrepaidRequest());
        var url = $"{RecognitionUrl}/{schedule.Id}/generate";
        var request = new GenerateRecognitionRequest(1);
        var responses = await Task.WhenAll(
            first.PostAsJsonAsync(url, request),
            second.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(responses.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join("; ", await Task.WhenAll(
                    responses.Select(x => x.Content.ReadAsStringAsync()))));
            var results = await Task.WhenAll(
                responses.Select(x => x.Content
                    .ReadFromJsonAsync<RecognitionGenerationResponse>()));
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[1]);
            Assert.AreEqual(results[0]!.JournalEntryId,
                results[1]!.JournalEntryId);
            Assert.AreNotEqual(results[0].AlreadyGenerated,
                results[1].AlreadyGenerated);
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA &&
            x.ExternalReference == $"recognition-schedule:v1:{schedule.Id:N}:1"));
    }
}
