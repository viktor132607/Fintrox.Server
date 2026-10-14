using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task AnnualLedgerReconciliationExposesUnclosedIncomeAndBalanceTotals()
    {
        var calendar = await PrepareYearEndCalendarAsync();
        using var client = Client(userA, organizationA);
        await PostProfitLossTransactionAsync(client, revenue: 75m, expense: 35m);
        await CloseMarchAsync(client, calendar.YearId, calendar.MarchPeriodId);
        var baseUrl = $"/api/v1/accounting/fiscal-years/{calendar.YearId}";

        using var first = await client.GetAsync($"{baseUrl}/close-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        var before = await first.Content
            .ReadFromJsonAsync<FiscalCloseReconciliationResponse>();
        Assert.IsNotNull(before);
        Assert.IsTrue(before.LedgerBalanced);
        Assert.IsTrue(before.CalendarContiguous);
        Assert.IsTrue(before.PeriodsReadyForClose);
        Assert.AreEqual(0, before.DraftJournalCount);
        Assert.IsTrue(before.ReadyForProfitLossClose);
        Assert.IsFalse(before.ProfitLossCleared);
        Assert.AreEqual(35m, before.OpenProfitLossDebit);
        Assert.AreEqual(75m, before.OpenProfitLossCredit);
        Assert.AreEqual(before.PostedDebitTotal, before.PostedCreditTotal);

        using (var closed = await client.PostAsJsonAsync(
            $"{baseUrl}/close-profit-loss", new CloseProfitLossRequest(retainedA)))
            Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode);

        using var afterResponse = await client.GetAsync(
            $"{baseUrl}/close-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, afterResponse.StatusCode);
        var after = await afterResponse.Content
            .ReadFromJsonAsync<FiscalCloseReconciliationResponse>();
        Assert.IsNotNull(after);
        Assert.IsTrue(after.LedgerBalanced);
        Assert.IsTrue(after.ProfitLossCleared);
        Assert.AreEqual(0m, after.OpenProfitLossDebit);
        Assert.AreEqual(0m, after.OpenProfitLossCredit);
        Assert.IsFalse(after.ReadyForProfitLossClose);
        Assert.AreEqual("Closed", after.FiscalYearStatus);
        Assert.IsNotNull(after.LatestClosingJournalEntryId);
    }

    [TestMethod]
    public async Task LedgerReconciliationExposesDraftsWithoutClaimingReadyToClose()
    {
        var calendar = await PrepareYearEndCalendarAsync();
        using var client = Client(userA, organizationA);
        await CreateDraftAsync(client, 13m, 13m);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{calendar.YearId}/close-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FiscalCloseReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.DraftJournalCount);
        Assert.IsFalse(report.ReadyForProfitLossClose);
        Assert.IsTrue(report.LedgerBalanced);
    }

    [TestMethod]
    public async Task ReopenCorrectionRejectsForeignTenantYear()
    {
        var calendar = await PrepareYearEndCalendarAsync();
        using var other = Client(userB, organizationB);
        using var response = await other.PostAsJsonAsync(
            $"/api/v1/accounting/fiscal-years/{calendar.YearId}/reopen-profit-loss",
            new ReopenProfitLossRequest(Guid.NewGuid(), "Reclassification following year end financial audit"));
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ConcurrentCorrectionRequestsProduceOneReversal()
    {
        var calendar = await PrepareYearEndCalendarAsync();
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        await PostProfitLossTransactionAsync(first, revenue: 100m, expense: 45m);
        await CloseMarchAsync(first, calendar.YearId, calendar.MarchPeriodId);
        var url = $"/api/v1/accounting/fiscal-years/{calendar.YearId}";
        using var closed = await first.PostAsJsonAsync(
            $"{url}/close-profit-loss", new CloseProfitLossRequest(retainedA));
        Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode);
        var closeResult = await closed.Content.ReadFromJsonAsync<ProfitLossCloseResponse>();
        Assert.IsNotNull(closeResult);

        const string reason = "Correcting annual expense accrual classification";
        var responses = await Task.WhenAll(
            first.PostAsJsonAsync($"{url}/reopen-profit-loss",
                new ReopenProfitLossRequest(closeResult.JournalEntryId!.Value, reason)),
            second.PostAsJsonAsync($"{url}/reopen-profit-loss",
                new ReopenProfitLossRequest(closeResult.JournalEntryId!.Value, reason)));
        try
        {
            Assert.IsTrue(responses.All(r => r.StatusCode == HttpStatusCode.OK),
                string.Join(" | ", await Task.WhenAll(
                    responses.Select(r => r.Content.ReadAsStringAsync()))));
            var records = await Task.WhenAll(responses.Select(r =>
                r.Content.ReadFromJsonAsync<ProfitLossReopenResponse>()));
            Assert.IsNotNull(records[0]);
            Assert.IsNotNull(records[1]);
            Assert.AreEqual(records[0]!.ReversalJournalEntryId,
                records[1]!.ReversalJournalEntryId);
            Assert.AreEqual(1, records.Count(r => r!.AlreadyReopened));
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }

        await using var db = CreateDb();
        Assert.AreEqual(1, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA &&
            x.ReversalOfJournalEntryId != null));
        Assert.AreEqual(FiscalYearStatus.Open,
            (await db.FiscalYears.SingleAsync(x => x.Id == calendar.YearId)).Status);
    }
}
