using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>HTTP + migrated disposable PostgreSQL tests, not mock-only tests.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task BankRevaluationPostsUnrealizedGainAndExactlyBalancesGL()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        var url = ValuationUrl(setup.StatementId);
        var request = RevaluationRequest(setup.RateId, 120m, 100m);
        using var response = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var result = await response.Content
            .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
        Assert.IsNotNull(result);
        Assert.IsFalse(result.AlreadyRevalued);
        Assert.IsNotNull(result.JournalEntryId);
        Assert.AreEqual(120m, result.TargetBaseBalance);
        Assert.AreEqual(20m, result.SignedAdjustmentBase);
        Assert.AreEqual(20m, result.UnrealizedGainBase);
        Assert.AreEqual(0m, result.UnrealizedLossBase);

        await using (var db = CreateDb())
        {
            var original = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == result.JournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Posted, original.Status);
            Assert.AreEqual($"bank-fx-revaluation:v1:{setup.BankId:N}:20260331",
                original.ExternalReference);
            var lines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == original.Id).ToArrayAsync();
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual(20m, lines.Sum(x => x.Debit));
            Assert.AreEqual(20m, lines.Sum(x => x.Credit));
            Assert.AreEqual(20m, lines.Single(x => x.AccountId == assetA).Debit);
            Assert.AreEqual(20m, lines.Single(x => x.AccountId == revenueA).Credit);
        }

        using var replay = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var previous = await replay.Content
            .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
        Assert.IsNotNull(previous);
        Assert.IsTrue(previous.AlreadyRevalued);
        Assert.AreEqual(result.JournalEntryId, previous.JournalEntryId);

        using var altered = await client.PostAsJsonAsync(
            url, request with { ExpectedLedgerBaseBalance = 105m });
        Assert.AreEqual(HttpStatusCode.Conflict, altered.StatusCode);
    }

    [TestMethod]
    public async Task BankRevaluationPostsUnrealizedLossAndCreditToBankAsset()
    {
        var setup = await SetupValuationAsync(1.5m);
        using var client = Client(userA, organizationA);
        using var response = await client.PostAsJsonAsync(
            ValuationUrl(setup.StatementId),
            RevaluationRequest(setup.RateId, 120m, 100m));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content
            .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
        Assert.IsNotNull(result);
        Assert.AreEqual(80m, result.TargetBaseBalance);
        Assert.AreEqual(-20m, result.SignedAdjustmentBase);
        Assert.AreEqual(20m, result.UnrealizedLossBase);
        Assert.AreEqual(0m, result.UnrealizedGainBase);
        await using var db = CreateDb();
        var rows = await db.JournalLines.AsNoTracking()
            .Where(x => x.JournalEntryId == result.JournalEntryId).ToArrayAsync();
        Assert.AreEqual(2, rows.Length);
        Assert.AreEqual(20m, rows.Single(x => x.AccountId == assetA).Credit);
        Assert.AreEqual(20m, rows.Single(x => x.AccountId == expenseA).Debit);
    }

    [TestMethod]
    public async Task ZeroFxRevaluationStoresEvidenceWithoutCreatingZeroJournal()
    {
        var setup = await SetupValuationAsync(1.2m);
        using var client = Client(userA, organizationA);
        var url = ValuationUrl(setup.StatementId);
        var request = RevaluationRequest(setup.RateId, 120m, 100m);
        using var response = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content
            .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
        Assert.IsNotNull(result);
        Assert.AreEqual(0m, result.SignedAdjustmentBase);
        Assert.IsNull(result.JournalEntryId);

        using var retry = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, retry.StatusCode);
        var again = await retry.Content
            .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
        Assert.IsNotNull(again);
        Assert.IsTrue(again.AlreadyRevalued);
        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(
            x => x.OrganizationId == organizationA &&
                x.ExternalReference != null &&
                x.ExternalReference.StartsWith("bank-fx-revaluation:v1:")));
    }

    [TestMethod]
    public async Task BankRevaluationRejectsUnsettledNativeBankLines()
    {
        var setup = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(client, setup.Bank.Id, 120m);
        var rate = await CreateEndRateAsync(1m);
        using var response = await client.PostAsJsonAsync(
            ValuationUrl(statement.Id), RevaluationRequest(rate, 120m, 0m));
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    [TestMethod]
    public async Task BankRevaluationRejectsUnapprovedRateAndIncorrectLedgerExpectation()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        using (var nonexistent = await client.PostAsJsonAsync(
            ValuationUrl(setup.StatementId),
            RevaluationRequest(Guid.NewGuid(), 120m, 100m)))
            Assert.AreEqual(HttpStatusCode.Conflict, nonexistent.StatusCode);
        using var wrongGl = await client.PostAsJsonAsync(
            ValuationUrl(setup.StatementId),
            RevaluationRequest(setup.RateId, 120m, 95m));
        Assert.AreEqual(HttpStatusCode.Conflict, wrongGl.StatusCode);
    }

    [TestMethod]
    public async Task BankRevaluationRejectsClosedPeriodAndOtherTenant()
    {
        var setup = await SetupValuationAsync(1m);
        using var owner = Client(userA, organizationA);
        using var foreign = Client(userB, organizationB);
        var path = ValuationUrl(setup.StatementId);
        var request = RevaluationRequest(setup.RateId, 120m, 100m);
        using (var denied = await foreign.PostAsJsonAsync(path, request))
            Assert.AreEqual(HttpStatusCode.NotFound, denied.StatusCode);

        var calendar = await MarchCalendarAsync(organizationA);
        using (var closed = await owner.PostAsync(
            $"/api/v1/accounting/fiscal-years/{calendar.YearId}/periods/{calendar.PeriodId}/close", null))
            Assert.AreEqual(HttpStatusCode.OK, closed.StatusCode);

        using var rejected = await owner.PostAsJsonAsync(path, request);
        Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
    }

    [TestMethod]
    public async Task BankRevaluationRejectsRetroactiveStatementImportAfterCutoff()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        using (var complete = await client.PostAsJsonAsync(
            ValuationUrl(setup.StatementId),
            RevaluationRequest(setup.RateId, 120m, 100m)))
            Assert.AreEqual(HttpStatusCode.OK, complete.StatusCode);

        var backdated = new ImportBankStatementRequest(
            "BACKDATED-FEB", new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28), 0m, 10m,
            [new BankStatementLineImportRequest(
                "EARLY-01", new DateOnly(2026, 2, 15), 10m,
                "Bank import with prior cutoff")]);
        using var denied = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{setup.BankId}/statements", backdated);
        Assert.AreEqual(HttpStatusCode.Conflict, denied.StatusCode);
    }

    [TestMethod]
    public async Task BankRevaluationConcurrentSameRequestHasOneJournal()
    {
        var setup = await SetupValuationAsync(1m);
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var url = ValuationUrl(setup.StatementId);
        var request = RevaluationRequest(setup.RateId, 120m, 100m);
        var replies = await Task.WhenAll(
            first.PostAsJsonAsync(url, request), second.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(replies.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join("; ", await Task.WhenAll(replies.Select(x =>
                    x.Content.ReadAsStringAsync()))));
            var a = await replies[0].Content
                .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
            var b = await replies[1].Content
                .ReadFromJsonAsync<ForeignBankRevaluationResponse>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.JournalEntryId, b.JournalEntryId);
            Assert.AreNotEqual(a.AlreadyRevalued, b.AlreadyRevalued);
        }
        finally
        {
            foreach (var reply in replies) reply.Dispose();
        }
    }

    private async Task<(Guid StatementId, Guid BankId, Guid RateId)>
        SetupValuationAsync(decimal closingQuoteRate)
    {
        var bankSetup = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(
            client, bankSetup.Bank.Id, 120m);
        var line = await BankLineIdAsync(statement.Id);
        using (var posted = await client.PostAsJsonAsync(
            FxUrl(statement.Id, line),
            FxRequest(bankSetup, 120m, 95m)))
            Assert.AreEqual(HttpStatusCode.OK, posted.StatusCode,
                await posted.Content.ReadAsStringAsync());
        var rate = await CreateEndRateAsync(closingQuoteRate);
        return (statement.Id, bankSetup.Bank.Id, rate);
    }

    private async Task<Guid> CreateEndRateAsync(decimal quotePerBase)
    {
        await using var db = CreateDb();
        var eur = await db.Currencies.SingleAsync(x =>
            x.OrganizationId == organizationA && x.Code == "EUR");
        var usd = await db.Currencies.SingleAsync(x =>
            x.OrganizationId == organizationA && x.Code == "USD");
        var rate = ExchangeRate.Create(
            organizationA, eur.Id, usd.Id,
            new DateOnly(2026, 3, 31), quotePerBase,
            "Test approved cutoff rate", DateTimeOffset.UtcNow);
        db.ExchangeRates.Add(rate);
        await db.SaveChangesAsync();
        return rate.Id;
    }

    private static string ValuationUrl(Guid statementId) =>
        $"/api/v1/banking/statements/{statementId}/revalue-foreign";

    private PostForeignBankRevaluationRequest RevaluationRequest(
        Guid rateId, decimal native, decimal bankBase) =>
        new(rateId, revenueA, expenseA, native, bankBase);
}
