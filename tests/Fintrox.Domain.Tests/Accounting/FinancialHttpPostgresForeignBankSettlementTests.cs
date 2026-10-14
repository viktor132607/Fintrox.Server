using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>Actual HTTP + disposable migrated PostgreSQL foreign-bank tests.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task IncomingForeignSettlementPostsRealizedGainAndNeverNaivelyComparesCurrencies()
    {
        var config = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(client, config.Bank.Id, 120m);
        var lineId = await BankLineIdAsync(statement.Id);
        var request = FxRequest(config, 120m, 95m);
        using var result = await client.PostAsJsonAsync(FxUrl(statement.Id, lineId), request);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode,
            await result.Content.ReadAsStringAsync());
        var posted = await result.Content.ReadFromJsonAsync<ForeignBankSettlementResponse>();
        Assert.IsNotNull(posted);
        Assert.AreEqual(100m, posted.BankBaseAmount);
        Assert.AreEqual(5m, posted.RealizedGainBase);
        Assert.AreEqual(0m, posted.RealizedLossBase);
        Assert.AreEqual(1.2m, posted.BookedRate);

        await using (var db = CreateDb())
        {
            var journal = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == posted.JournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Posted, journal.Status);
            Assert.AreEqual($"bank-fx-settlement:v1:{lineId:N}", journal.ExternalReference);
            var rows = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == journal.Id).ToArrayAsync();
            Assert.AreEqual(3, rows.Length);
            Assert.AreEqual(100m, rows.Sum(x => x.Debit));
            Assert.AreEqual(100m, rows.Sum(x => x.Credit));
            Assert.AreEqual(100m, rows.Single(x => x.AccountId == assetA).Debit);
            Assert.AreEqual(95m, rows.Single(x => x.AccountId == config.ClearingId).Credit);
            Assert.AreEqual(5m, rows.Single(x => x.AccountId == revenueA).Credit);
        }

        using var detailResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}");
        var details = await detailResponse.Content.ReadFromJsonAsync<BankStatementResponse>();
        Assert.IsNotNull(details);
        Assert.AreEqual(posted.JournalEntryId,
            details.Lines.Single().ForeignSettlementJournalEntryId);

        using var reportResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, reportResponse.StatusCode);
        var report = await reportResponse.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.PostedForeignSettlementLines);
        Assert.AreEqual(0, report.UnmatchedLines);
        Assert.IsFalse(report.BankAndLedgerCurrencyComparable);
        Assert.IsFalse(report.BankAndLedgerBalanced);
        Assert.IsFalse(report.FullyReconciled);
        Assert.IsNull(report.StatementVsLedgerDifference);
        Assert.AreEqual(100m, report.GeneralLedgerClosingBalance);

        using var again = await client.PostAsJsonAsync(FxUrl(statement.Id, lineId), request);
        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
        var replay = await again.Content.ReadFromJsonAsync<ForeignBankSettlementResponse>();
        Assert.IsNotNull(replay);
        Assert.IsTrue(replay.AlreadyPosted);
        Assert.AreEqual(posted.JournalEntryId, replay.JournalEntryId);
    }

    [TestMethod]
    public async Task OutgoingForeignSettlementPostsRealizedLoss()
    {
        var config = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(client, config.Bank.Id, -120m);
        var line = await BankLineIdAsync(statement.Id);
        using var response = await client.PostAsJsonAsync(
            FxUrl(statement.Id, line), FxRequest(config, -120m, 95m));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var posted = await response.Content.ReadFromJsonAsync<ForeignBankSettlementResponse>();
        Assert.IsNotNull(posted);
        Assert.AreEqual(5m, posted.RealizedLossBase);
        Assert.AreEqual(0m, posted.RealizedGainBase);
        await using var db = CreateDb();
        var lines = await db.JournalLines.AsNoTracking()
            .Where(x => x.JournalEntryId == posted.JournalEntryId).ToArrayAsync();
        Assert.AreEqual(100m, lines.Sum(x => x.Debit));
        Assert.AreEqual(100m, lines.Sum(x => x.Credit));
        Assert.AreEqual(100m, lines.Single(x => x.AccountId == assetA).Credit);
        Assert.AreEqual(95m, lines.Single(x => x.AccountId == config.ClearingId).Debit);
        Assert.AreEqual(5m, lines.Single(x => x.AccountId == expenseA).Debit);
    }

    [TestMethod]
    public async Task ForeignSettlementRejectsWrongRateAndChangedReplay()
    {
        var config = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(client, config.Bank.Id, 120m);
        var line = await BankLineIdAsync(statement.Id);
        var url = FxUrl(statement.Id, line);
        using (var wrongAmount = await client.PostAsJsonAsync(
            url, FxRequest(config, 119m, 95m)))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongAmount.StatusCode);
        using (var wrongRate = await client.PostAsJsonAsync(
            url, FxRequest(config, 120m, 95m) with { ExchangeRateId = Guid.NewGuid() }))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongRate.StatusCode);
        using (var original = await client.PostAsJsonAsync(
            url, FxRequest(config, 120m, 95m)))
            Assert.AreEqual(HttpStatusCode.OK, original.StatusCode);
        using var different = await client.PostAsJsonAsync(
            url, FxRequest(config, 120m, 90m));
        Assert.AreEqual(HttpStatusCode.Conflict, different.StatusCode);
    }

    [TestMethod]
    public async Task ForeignStatementIsRejectedByOldBaseCurrencyPaymentAndFeeRoutes()
    {
        var config = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(client, config.Bank.Id, -120m);
        var line = await BankLineIdAsync(statement.Id);
        using (var fee = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/post-fee",
            new PostBankFeeRequest(expenseA, -120m, "Incorrect foreign fee classification")))
            Assert.AreEqual(HttpStatusCode.Conflict, fee.StatusCode);
        using (var match = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/match",
            new MatchBankStatementLineRequest(Guid.NewGuid())))
            Assert.AreEqual(HttpStatusCode.Conflict, match.StatusCode);
        using var suggested = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/match-suggestions");
        var proposals = await suggested.Content
            .ReadFromJsonAsync<BankMatchSuggestionsResponse>();
        Assert.IsNotNull(proposals);
        Assert.AreEqual(0, proposals.UnambiguousSuggestions);
    }

    [TestMethod]
    public async Task ForeignSettlementRejectsCrossTenantAccessAndClosedFiscalPeriod()
    {
        var config = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        using var foreign = Client(userB, organizationB);
        var statement = await ImportFxStatementAsync(client, config.Bank.Id, 120m);
        var line = await BankLineIdAsync(statement.Id);
        var url = FxUrl(statement.Id, line);
        using (var rejected = await foreign.PostAsJsonAsync(
            url, FxRequest(config, 120m, 95m)))
            Assert.AreEqual(HttpStatusCode.NotFound, rejected.StatusCode);

        var year = await MarchCalendarAsync(organizationA);
        using (var close = await client.PostAsync(
            $"/api/v1/accounting/fiscal-years/{year.YearId}/periods/{year.PeriodId}/close",
            null))
            Assert.AreEqual(HttpStatusCode.OK, close.StatusCode);
        using var afterClose = await client.PostAsJsonAsync(
            url, FxRequest(config, 120m, 95m));
        Assert.AreEqual(HttpStatusCode.Conflict, afterClose.StatusCode);
    }

    [TestMethod]
    public async Task ForeignSettlementDoubleSubmissionProducesOneJournal()
    {
        var config = await SetupFxBankAsync();
        using var first = Client(userA, organizationA);
        using var second = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(first, config.Bank.Id, 120m);
        var line = await BankLineIdAsync(statement.Id);
        var url = FxUrl(statement.Id, line);
        var request = FxRequest(config, 120m, 95m);
        var responses = await Task.WhenAll(first.PostAsJsonAsync(url, request),
            second.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(responses.All(x => x.StatusCode == HttpStatusCode.OK));
            var a = await responses[0].Content.ReadFromJsonAsync<ForeignBankSettlementResponse>();
            var b = await responses[1].Content.ReadFromJsonAsync<ForeignBankSettlementResponse>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.JournalEntryId, b.JournalEntryId);
            Assert.AreNotEqual(a.AlreadyPosted, b.AlreadyPosted);
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
    }

    private async Task<(BankAccountResponse Bank, Guid RateId, Guid ClearingId)>
        SetupFxBankAsync()
    {
        Guid rateId;
        Guid clearingId;
        await using (var db = CreateDb())
        {
            var now = DateTimeOffset.UtcNow;
            var eur = Currency.Create(
                organizationA, "EUR", "Euro", "€", 2, true, now);
            var usd = Currency.Create(
                organizationA, "USD", "US Dollar", "$", 2, false, now);
            var rate = ExchangeRate.Create(
                organizationA, eur.Id, usd.Id, PostingDate, 1.2m, "Test ECB", now);
            var clearing = Account.Create(
                organizationA, "411-FX", "FX receivable/payable clearing",
                AccountType.Asset, null, true, true, now);
            db.Currencies.AddRange(eur, usd);
            db.ExchangeRates.Add(rate);
            db.Accounts.Add(clearing);
            await db.SaveChangesAsync();
            rateId = rate.Id;
            clearingId = clearing.Id;
        }
        using var client = Client(userA, organizationA);
        using var created = await client.PostAsJsonAsync(
            "/api/v1/banking/accounts", new CreateBankAccountRequest(
                "BG22BANK01234567890123", "USD foreign bank", "USD", assetA));
        Assert.AreEqual(HttpStatusCode.Created, created.StatusCode,
            await created.Content.ReadAsStringAsync());
        var account = await created.Content.ReadFromJsonAsync<BankAccountResponse>();
        Assert.IsNotNull(account);
        return (account, rateId, clearingId);
    }

    private PostForeignBankSettlementRequest FxRequest(
        (BankAccountResponse Bank, Guid RateId, Guid ClearingId) config,
        decimal native, decimal carrying) =>
        new(config.RateId, config.ClearingId, carrying,
            revenueA, expenseA, native);

    private static string FxUrl(Guid statementId, Guid lineId) =>
        $"/api/v1/banking/statements/{statementId}/lines/{lineId}/post-foreign-settlement";

    private static async Task<BankStatementImportResponse> ImportFxStatementAsync(
        HttpClient client, Guid bankId, decimal native)
    {
        using var result = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bankId}/statements",
            new ImportBankStatementRequest(
                "FX-MARCH-001", new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31), 0m, native,
                [new BankStatementLineImportRequest(
                    "FX-LINE-001", PostingDate, native, "Foreign bank transfer")]));
        Assert.AreEqual(HttpStatusCode.Created, result.StatusCode,
            await result.Content.ReadAsStringAsync());
        var imported = await result.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(imported);
        return imported;
    }
}
