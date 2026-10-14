using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Fintrox.Contracts.Accounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Full HTTP integration on migrated disposable PostgreSQL.
/// The Banking migration is executed by the shared fixture.
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task BankStatementImportAndPaymentMatchReconcileWithGL()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var source = Statement("ST-2026-03-001", 100m);

        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements", source);
        Assert.AreEqual(HttpStatusCode.Created, imported.StatusCode,
            await imported.Content.ReadAsStringAsync());
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        Assert.IsFalse(statement.AlreadyImported);

        var line = await BankLineIdAsync(statement.Id);
        using var firstMatch = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/match",
            new MatchBankStatementLineRequest(paymentId));
        Assert.AreEqual(HttpStatusCode.OK, firstMatch.StatusCode,
            await firstMatch.Content.ReadAsStringAsync());

        using var reportResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, reportResponse.StatusCode);
        var report = await reportResponse.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsTrue(report.StatementsBalanced);
        Assert.IsTrue(report.BankAndLedgerBalanced);
        Assert.IsTrue(report.StatementLinesAllMatched);
        Assert.IsTrue(report.FullyReconciled);
        Assert.AreEqual(1, report.MatchedLines);
        Assert.AreEqual(0, report.UnmatchedLines);
        Assert.AreEqual(0, report.UnclearedBookPayments);
        Assert.AreEqual(100m, report.GeneralLedgerClosingBalance);
        Assert.AreEqual(0m, report.StatementVsLedgerDifference);

        using var retry = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/match",
            new MatchBankStatementLineRequest(paymentId));
        Assert.AreEqual(HttpStatusCode.OK, retry.StatusCode);
        Assert.IsTrue((await retry.Content
            .ReadFromJsonAsync<BankStatementMatchResponse>())!.AlreadyMatched);
    }

    [TestMethod]
    public async Task BankStatementImportIsIdempotentAndChangedReplayConflicts()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var request = Statement("STATEMENT-IDEMPOTENT-01", 20m);
        var path = $"/api/v1/banking/accounts/{bank.Id}/statements";

        using var first = await client.PostAsJsonAsync(path, request);
        Assert.AreEqual(HttpStatusCode.Created, first.StatusCode);
        var initial = await first.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(initial);

        using var second = await client.PostAsJsonAsync(path, request);
        Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
        var replay = await second.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(replay);
        Assert.IsTrue(replay.AlreadyImported);
        Assert.AreEqual(initial.Id, replay.Id);

        using var changed = await client.PostAsJsonAsync(
            path, Statement("STATEMENT-IDEMPOTENT-01", 30m));
        Assert.AreEqual(HttpStatusCode.Conflict, changed.StatusCode);
        await using var cn = new NpgsqlConnection(connectionString);
        await cn.OpenAsync();
        await using var count = new NpgsqlCommand(
            "SELECT count(*) FROM banking.bank_statements", cn);
        Assert.AreEqual(1L, (long)(await count.ExecuteScalarAsync())!);
    }

    [TestMethod]
    public async Task BankStatementImportRejectsWrongBalanceAndDuplicateLines()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var url = $"/api/v1/banking/accounts/{bank.Id}/statements";
        var unbalanced = Statement("BAD-BALANCE", 10m) with
        {
            ClosingBalance = 15m
        };
        using var bad = await client.PostAsJsonAsync(url, unbalanced);
        Assert.AreEqual(HttpStatusCode.Conflict, bad.StatusCode);

        var duplicated = Statement("BAD-REPEAT", 10m) with
        {
            ClosingBalance = 20m,
            Lines =
            [
                new BankStatementLineImportRequest(
                    "LINE-1", PostingDate, 10m, "First"),
                new BankStatementLineImportRequest(
                    "LINE-1", PostingDate, 10m, "Repeated")
            ]
        };
        using var repeated = await client.PostAsJsonAsync(url, duplicated);
        Assert.AreEqual(HttpStatusCode.Conflict, repeated.StatusCode);
    }

    [TestMethod]
    public async Task BankStatementRejectsWrongAmountAndPaymentAlreadyMatched()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("MATCH-CHECK", 70m));
        Assert.AreEqual(HttpStatusCode.Created, imported.StatusCode);
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        var line = await BankLineIdAsync(statement.Id);

        using var bad = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/match",
            new MatchBankStatementLineRequest(paymentId));
        Assert.AreEqual(HttpStatusCode.Conflict, bad.StatusCode);

        using var response = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsFalse(report.FullyReconciled);
        Assert.AreEqual(1, report.UnmatchedLines);
        Assert.AreEqual(1, report.UnclearedBookPayments);
        Assert.AreEqual(-30m, report.StatementVsLedgerDifference);
    }

    [TestMethod]
    public async Task BankingCannotAccessAnotherOrganizationsAccountOrStatement()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var bank = await CreateTestBankAccountAsync(owner);
        using var foreignImport = await other.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("FOREIGN-STATEMENT", 8m));
        Assert.AreEqual(HttpStatusCode.NotFound, foreignImport.StatusCode);

        using var imported = await owner.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("OWN-STATEMENT", 8m));
        Assert.AreEqual(HttpStatusCode.Created, imported.StatusCode);
        var source = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(source);

        using var foreignRead = await other.GetAsync(
            $"/api/v1/banking/statements/{source.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.NotFound, foreignRead.StatusCode);
        using var foreignMatch = await other.PostAsJsonAsync(
            $"/api/v1/banking/statements/{source.Id}/lines/{Guid.NewGuid()}/match",
            new MatchBankStatementLineRequest(Guid.NewGuid()));
        Assert.AreEqual(HttpStatusCode.NotFound, foreignMatch.StatusCode);
    }

    [TestMethod]
    public async Task BankAccountRejectsNonAssetLedgerAndNonBaseCurrency()
    {
        using var client = Client(userA, organizationA);
        using var wrongAccount = await client.PostAsJsonAsync(
            "/api/v1/banking/accounts", new CreateBankAccountRequest(
                "BG22BANK01234567890123", "Wrong Ledger", "EUR", revenueA));
        Assert.AreEqual(HttpStatusCode.Conflict, wrongAccount.StatusCode);

        using var foreignCurrency = await client.PostAsJsonAsync(
            "/api/v1/banking/accounts", new CreateBankAccountRequest(
                "BG22BANK01234567890124", "USD Bank", "USD", assetA));
        Assert.AreEqual(HttpStatusCode.Conflict, foreignCurrency.StatusCode);
    }

    [TestMethod]
    public async Task BankMatchIsExclusiveAcrossMultipleStatements()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);

        using var march = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("MARCH-EXCLUSIVE", 100m));
        Assert.AreEqual(HttpStatusCode.Created, march.StatusCode);
        var marchStatement = await march.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(marchStatement);
        var marchLine = await BankLineIdAsync(marchStatement.Id);
        using (var first = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{marchStatement.Id}/lines/{marchLine}/match",
            new MatchBankStatementLineRequest(paymentId)))
            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);

        var april = new ImportBankStatementRequest(
            "APRIL-EXCLUSIVE", new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30), 0m, 100m,
            [new BankStatementLineImportRequest(
                "APRIL-1", new DateOnly(2026, 4, 3), 100m,
                "Duplicate receipt on a different bank statement")]);
        using var secondImport = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements", april);
        Assert.AreEqual(HttpStatusCode.Created, secondImport.StatusCode);
        var later = await secondImport.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(later);

        var aprilLine = await BankLineIdAsync(later.Id);
        using var duplicate = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{later.Id}/lines/{aprilLine}/match",
            new MatchBankStatementLineRequest(paymentId));
        Assert.AreEqual(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [TestMethod]
    public async Task BankReportFailsWhenMatchedPaymentIsLaterCancelled()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(organizationA, $"payment:{paymentId:N}", 100m);
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        using var imported = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("CANCELLED-MATCH", 100m));
        var statement = await imported.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(statement);
        var line = await BankLineIdAsync(statement.Id);
        using (var match = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{line}/match",
            new MatchBankStatementLineRequest(paymentId)))
            Assert.AreEqual(HttpStatusCode.OK, match.StatusCode);

        await using (var db = CreateDb())
        {
            var payment = await db.Payments.SingleAsync(p => p.Id == paymentId);
            payment.Cancel("Reversed after bank reconciliation", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        using var result = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        var report = await result.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsFalse(report.FullyReconciled);
        Assert.AreEqual(1, report.UnmatchedLines);
        Assert.IsTrue(report.UnmatchedLineIds.Contains(line));
    }

    private async Task<BankAccountResponse> CreateTestBankAccountAsync(
        HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/banking/accounts",
            new CreateBankAccountRequest(
                "BG22BANK01234567890123", "Operating account", "EUR", assetA));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var bank = await response.Content
            .ReadFromJsonAsync<BankAccountResponse>();
        Assert.IsNotNull(bank);
        return bank;
    }

    private static ImportBankStatementRequest Statement(
        string external, decimal incoming) =>
        new(external, new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31), 0m, incoming,
            [new BankStatementLineImportRequest(
                "LINE-1", PostingDate, incoming, "Incoming bank payment")]);

    private async Task<Guid> BankLineIdAsync(Guid statementId)
    {
        await using var cn = new NpgsqlConnection(connectionString);
        await cn.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id FROM banking.bank_statement_lines WHERE statement_id=@id",
            cn);
        command.Parameters.AddWithValue("id", statementId);
        return (Guid)(await command.ExecuteScalarAsync())!;
    }
}
