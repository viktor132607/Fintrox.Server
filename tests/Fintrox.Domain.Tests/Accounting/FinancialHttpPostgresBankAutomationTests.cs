using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>Runs with the same isolated HTTP host and migrated PostgreSQL fixture.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task CsvBankImportHandlesQuotedDescriptionAndIdempotentReplay()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var request = CsvStatement("CSV-IMPORT-001",
            "externalId,valueDate,signedAmount,description\r\n" +
            "TRANSFER-1,2026-03-12,25.5000,\"Bank fee, transfer \"\"quoted\"\"\"\r\n");

        var path = $"/api/v1/banking/accounts/{bank.Id}/statements/import-csv";
        using var first = await client.PostAsJsonAsync(path, request);
        Assert.AreEqual(HttpStatusCode.Created, first.StatusCode,
            await first.Content.ReadAsStringAsync());
        var imported = await first.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(imported);
        Assert.AreEqual(1, imported.LineCount);

        using var read = await client.GetAsync(
            $"/api/v1/banking/statements/{imported.Id}");
        Assert.AreEqual(HttpStatusCode.OK, read.StatusCode);
        var detail = await read.Content.ReadFromJsonAsync<BankStatementResponse>();
        Assert.IsNotNull(detail);
        Assert.HasCount(1, detail.Lines);
        Assert.AreEqual("Bank fee, transfer \"quoted\"", detail.Lines[0].Description);
        Assert.AreEqual(25.5000m, detail.Lines[0].SignedAmount);

        using var replay = await client.PostAsJsonAsync(path, request);
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var repeated = await replay.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(repeated);
        Assert.IsTrue(repeated.AlreadyImported);
        Assert.AreEqual(imported.Id, repeated.Id);
    }

    [TestMethod]
    public async Task CsvImportRejectsInvalidHeaderDateAndBrokenQuotes()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var path = $"/api/v1/banking/accounts/{bank.Id}/statements/import-csv";
        string[] invalid =
        [
            "id,valueDate,signedAmount,description\nX,2026-03-12,5,fee",
            "externalId,valueDate,signedAmount,description\nX,12/03/2026,5,fee",
            "externalId,valueDate,signedAmount,description\nX,2026-03-12,1,000,fee",
            "externalId,valueDate,signedAmount,description\nX,2026-03-12,5,\"broken"
        ];
        foreach (var csv in invalid)
        {
            using var response = await client.PostAsJsonAsync(path,
                CsvStatement("BAD-CSV", csv));
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode,
                csv);
        }
    }

    [TestMethod]
    public async Task ExactAutoMatchPostsOneSuggestedPaymentAndIsReplaySafe()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements/import-csv",
            CsvStatement("CSV-AUTO-001",
                "externalId,valueDate,signedAmount,description\n" +
                $"RECEIPT-1,{PostingDate:yyyy-MM-dd},100.0000,Client receipt\n",
                closingBalance: 100m));
        Assert.AreEqual(HttpStatusCode.Created, imported.StatusCode);
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        var path = $"/api/v1/banking/statements/{statement.Id}";
        using var suggestions = await client.GetAsync($"{path}/match-suggestions");
        Assert.AreEqual(HttpStatusCode.OK, suggestions.StatusCode);
        var candidate = await suggestions.Content
            .ReadFromJsonAsync<BankMatchSuggestionsResponse>();
        Assert.IsNotNull(candidate);
        Assert.AreEqual(1, candidate.UnmatchedStatementLines);
        Assert.AreEqual(1, candidate.UnambiguousSuggestions);
        Assert.AreEqual(paymentId, candidate.Suggestions.Single().PaymentId);

        using var automate = await client.PostAsync($"{path}/auto-match", null);
        Assert.AreEqual(HttpStatusCode.OK, automate.StatusCode);
        var result = await automate.Content
            .ReadFromJsonAsync<AutoMatchBankStatementResponse>();
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Matched);
        Assert.AreEqual(0, result.Skipped);

        using var again = await client.PostAsync($"{path}/auto-match", null);
        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
        var replay = await again.Content
            .ReadFromJsonAsync<AutoMatchBankStatementResponse>();
        Assert.IsNotNull(replay);
        Assert.AreEqual(0, replay.Proposed);

        using var reconciliation = await client.GetAsync($"{path}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, reconciliation.StatusCode);
        var report = await reconciliation.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsTrue(report.FullyReconciled);
    }

    [TestMethod]
    public async Task ExactAutoMatchRefusesOnePaymentAcrossTwoStatementLines()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var request = new ImportBankStatementRequest(
            "DUPLICATE-SUGGESTIONS", new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31), 0m, 200m,
            [
                new BankStatementLineImportRequest(
                    "RECEIPT-A", PostingDate, 100m, "Receipt A"),
                new BankStatementLineImportRequest(
                    "RECEIPT-B", PostingDate, 100m, "Receipt B")
            ]);
        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements", request);
        Assert.AreEqual(HttpStatusCode.Created, imported.StatusCode);
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        var basePath = $"/api/v1/banking/statements/{statement.Id}";

        using var suggestions = await client.GetAsync(
            $"{basePath}/match-suggestions");
        Assert.AreEqual(HttpStatusCode.OK, suggestions.StatusCode);
        var proposal = await suggestions.Content
            .ReadFromJsonAsync<BankMatchSuggestionsResponse>();
        Assert.IsNotNull(proposal);
        Assert.AreEqual(2, proposal.UnmatchedStatementLines);
        Assert.AreEqual(0, proposal.UnambiguousSuggestions);

        using var automatic = await client.PostAsync(
            $"{basePath}/auto-match", null);
        Assert.AreEqual(HttpStatusCode.OK, automatic.StatusCode);
        var result = await automatic.Content
            .ReadFromJsonAsync<AutoMatchBankStatementResponse>();
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Matched);

        using var statementResponse = await client.GetAsync(basePath);
        var detail = await statementResponse.Content
            .ReadFromJsonAsync<BankStatementResponse>();
        Assert.IsNotNull(detail);
        Assert.IsTrue(detail.Lines.All(line => line.MatchedPaymentId is null));
    }

    [TestMethod]
    public async Task BankAutoMatchDeniesForeignTenantStatement()
    {
        using var client = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var bank = await CreateTestBankAccountAsync(client);
        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("EXCLUSIVE-TENANT", 10m));
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        var path = $"/api/v1/banking/statements/{statement.Id}";
        using var suggestions = await other.GetAsync(
            $"{path}/match-suggestions");
        using var auto = await other.PostAsync($"{path}/auto-match", null);
        Assert.AreEqual(HttpStatusCode.NotFound, suggestions.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, auto.StatusCode);
    }

    private static ImportCsvBankStatementRequest CsvStatement(
        string externalId, string csv, decimal closingBalance = 25.5000m)
    {
        return new ImportCsvBankStatementRequest(
            externalId, new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31), 0m,
            closingBalance, csv);
    }
}
