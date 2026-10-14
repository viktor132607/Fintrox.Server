using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Migrated PostgreSQL + authenticated Web API fixture from the finance suite.
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task BankFeePostsBalancedExpenseAndBankGLAndReconcilesStatement()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "FEE-MARCH-1", -5m);
        var lineId = await BankLineIdAsync(statement.Id);
        var url = FeeUrl(statement.Id, lineId);
        var request = new PostBankFeeRequest(
            expenseA, -5m, "Monthly account maintenance fee");

        using var response = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var posted = await response.Content.ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(posted);
        Assert.IsFalse(posted.AlreadyPosted);
        Assert.AreEqual(5m, posted.FeeAmount);
        Assert.AreEqual(expenseA, posted.ExpenseAccountId);

        await using (var db = CreateDb())
        {
            var journal = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == posted.JournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Posted, journal.Status);
            Assert.AreEqual(JournalEntrySource.System, journal.Source);
            Assert.AreEqual($"bank-fee:v1:{lineId:N}", journal.ExternalReference);
            Assert.AreEqual(PostingDate, journal.PostingDate);
            var journalLines = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == posted.JournalEntryId)
                .OrderBy(x => x.LineNumber).ToArrayAsync();
            Assert.HasCount(2, journalLines);
            Assert.AreEqual(5m, journalLines[0].Debit);
            Assert.AreEqual(0m, journalLines[0].Credit);
            Assert.AreEqual(expenseA, journalLines[0].AccountId);
            Assert.AreEqual(0m, journalLines[1].Debit);
            Assert.AreEqual(5m, journalLines[1].Credit);
            Assert.AreEqual(assetA, journalLines[1].AccountId);
        }

        using var detailsResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}");
        Assert.AreEqual(HttpStatusCode.OK, detailsResponse.StatusCode);
        var details = await detailsResponse.Content
            .ReadFromJsonAsync<BankStatementResponse>();
        Assert.IsNotNull(details);
        Assert.AreEqual(posted.JournalEntryId,
            details.Lines.Single().FeeJournalEntryId);
        Assert.IsNull(details.Lines.Single().MatchedPaymentId);

        using var reconciliationResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, reconciliationResponse.StatusCode);
        var report = await reconciliationResponse.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.PostedFeeLines);
        Assert.AreEqual(0, report.MatchedLines);
        Assert.AreEqual(0, report.UnmatchedLines);
        Assert.AreEqual(-5m, report.GeneralLedgerClosingBalance);
        Assert.AreEqual(0m, report.StatementVsLedgerDifference);
        Assert.IsTrue(report.FullyReconciled);

        using var replay = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var again = await replay.Content
            .ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(again);
        Assert.IsTrue(again.AlreadyPosted);
        Assert.AreEqual(posted.JournalEntryId, again.JournalEntryId);

        await using var finalDb = CreateDb();
        Assert.AreEqual(1, await finalDb.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA &&
            x.ExternalReference == $"bank-fee:v1:{lineId:N}"));
    }

    [TestMethod]
    public async Task BankFeeRejectsAmountAndExpenseAccountMismatch()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "FEE-VALIDATION-1", -10m);
        var lineId = await BankLineIdAsync(statement.Id);
        var url = FeeUrl(statement.Id, lineId);

        using (var wrongAmount = await client.PostAsJsonAsync(url,
            new PostBankFeeRequest(expenseA, -9m,
                "Monthly maintenance fee")))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongAmount.StatusCode);

        using (var wrongAccount = await client.PostAsJsonAsync(url,
            new PostBankFeeRequest(revenueA, -10m,
                "Monthly maintenance fee")))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongAccount.StatusCode);

        using (var bankAsExpense = await client.PostAsJsonAsync(url,
            new PostBankFeeRequest(assetA, -10m,
                "Monthly maintenance fee")))
            Assert.AreEqual(HttpStatusCode.Conflict, bankAsExpense.StatusCode);

        await using var db = CreateDb();
        Assert.AreEqual(0, await db.JournalEntries.CountAsync(x =>
            x.OrganizationId == organizationA &&
            x.ExternalReference == $"bank-fee:v1:{lineId:N}"));
    }

    [TestMethod]
    public async Task BankFeeRejectsPositiveBankLineAndChangedReplayReason()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        using (var positive = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bank.Id}/statements",
            Statement("POSITIVE-NOT-FEE", 8m)))
        {
            Assert.AreEqual(HttpStatusCode.Created, positive.StatusCode);
            var imported = await positive.Content
                .ReadFromJsonAsync<BankStatementImportResponse>();
            Assert.IsNotNull(imported);
            var lineId = await BankLineIdAsync(imported.Id);
            using var rejected = await client.PostAsJsonAsync(
                FeeUrl(imported.Id, lineId),
                new PostBankFeeRequest(expenseA, -8m,
                    "Bank fee with wrong sign"));
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
        }

        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "FEE-REASON-REPLAY", -4m);
        var line = await BankLineIdAsync(statement.Id);
        var url = FeeUrl(statement.Id, line);
        using (var original = await client.PostAsJsonAsync(
            url, new PostBankFeeRequest(
                expenseA, -4m, "Original bank service charge")))
            Assert.AreEqual(HttpStatusCode.OK, original.StatusCode);
        using var changed = await client.PostAsJsonAsync(
            url, new PostBankFeeRequest(
                expenseA, -4m, "Altered bank service charge"));
        Assert.AreEqual(HttpStatusCode.Conflict, changed.StatusCode);
    }

    [TestMethod]
    public async Task BankFeeCannotAlsoMatchAPaymentOrBeAutoSuggested()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "EXCLUSIVE-FEE", -5m);
        var lineId = await BankLineIdAsync(statement.Id);
        using var posted = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, lineId),
            new PostBankFeeRequest(expenseA, -5m,
                "Service fee from operating account"));
        Assert.AreEqual(HttpStatusCode.OK, posted.StatusCode);

        using var matched = await client.PostAsJsonAsync(
            $"/api/v1/banking/statements/{statement.Id}/lines/{lineId}/match",
            new MatchBankStatementLineRequest(Guid.NewGuid()));
        Assert.AreEqual(HttpStatusCode.Conflict, matched.StatusCode);

        using var suggestions = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/match-suggestions");
        Assert.AreEqual(HttpStatusCode.OK, suggestions.StatusCode);
        var found = await suggestions.Content
            .ReadFromJsonAsync<BankMatchSuggestionsResponse>();
        Assert.IsNotNull(found);
        Assert.AreEqual(0, found.UnambiguousSuggestions);
    }

    [TestMethod]
    public async Task BankFeeCannotPostIntoClosedAccountingPeriod()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "CLOSED-FEE", -6m);
        var lineId = await BankLineIdAsync(statement.Id);
        var fiscal = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(client, fiscal.YearId, fiscal.PeriodId);

        using var rejected = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, lineId),
            new PostBankFeeRequest(expenseA, -6m,
                "Fee for locked accounting period"));
        Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
    }

    [TestMethod]
    public async Task BankFeeIsTenantScopedAndIdempotentUnderParallelRequests()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        var bank = await CreateTestBankAccountAsync(owner);
        var statement = await ImportBankFeeStatementAsync(
            owner, bank.Id, "CONCURRENT-FEE", -7m);
        var lineId = await BankLineIdAsync(statement.Id);
        var url = FeeUrl(statement.Id, lineId);
        var request = new PostBankFeeRequest(
            expenseA, -7m, "Monthly bank statement management fee");

        using (var cross = await other.PostAsJsonAsync(url, request))
            Assert.AreEqual(HttpStatusCode.NotFound, cross.StatusCode);

        using var second = Client(userA, organizationA);
        var concurrent = await Task.WhenAll(
            owner.PostAsJsonAsync(url, request),
            second.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(concurrent.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join(" | ", await Task.WhenAll(
                    concurrent.Select(x => x.Content.ReadAsStringAsync()))));
            var results = await Task.WhenAll(concurrent.Select(x =>
                x.Content.ReadFromJsonAsync<BankFeePostingResponse>()));
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[1]);
            Assert.AreEqual(results[0]!.JournalEntryId, results[1]!.JournalEntryId);
            Assert.AreEqual(1, results.Count(x => x!.AlreadyPosted));
        }
        finally
        {
            foreach (var response in concurrent) response.Dispose();
        }
    }

    private static string FeeUrl(Guid statementId, Guid lineId) =>
        $"/api/v1/banking/statements/{statementId}/lines/{lineId}/post-fee";

    private static async Task<BankStatementImportResponse>
        ImportBankFeeStatementAsync(
            HttpClient client, Guid bankId, string externalId, decimal fee)
    {
        var request = new ImportBankStatementRequest(
            externalId, new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31), 0m, fee,
            [new BankStatementLineImportRequest(
                "FEE-LINE-1", PostingDate, fee, "Bank service charge")]);
        using var response = await client.PostAsJsonAsync(
            $"/api/v1/banking/accounts/{bankId}/statements", request);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var imported = await response.Content
            .ReadFromJsonAsync<BankStatementImportResponse>();
        Assert.IsNotNull(imported);
        return imported;
    }
}
