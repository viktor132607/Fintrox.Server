using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>Real Web API and migrated PostgreSQL compensation regressions.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task ControlledFeeCorrectionReversesPostedJournalAndLeavesUnresolvedBankEvidence()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "CORRECT-FEE-01", -5m);
        var lineId = await BankLineIdAsync(statement.Id);
        using var booked = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, lineId),
            new PostBankFeeRequest(expenseA, -5m,
                "Original monthly bank maintenance fee"));
        Assert.AreEqual(HttpStatusCode.OK, booked.StatusCode);
        var posted = await booked.Content
            .ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(posted);

        var request = new CorrectBankPostingRequest(
            "BankFee", posted.JournalEntryId, PostingDate,
            "Bank fee was assigned to incorrect expense category");
        using var response = await client.PostAsJsonAsync(
            CorrectionUrl(statement.Id, lineId), request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var corrected = await response.Content
            .ReadFromJsonAsync<BankPostingCorrectionResponse>();
        Assert.IsNotNull(corrected);
        Assert.IsFalse(corrected.AlreadyCorrected);
        Assert.AreEqual(posted.JournalEntryId, corrected.OriginalJournalEntryId);
        Assert.AreEqual(userA, corrected.CorrectedByUserId);

        await using (var db = CreateDb())
        {
            var source = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == posted.JournalEntryId);
            var compensation = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == corrected.ReversalJournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Reversed, source.Status);
            Assert.AreEqual(JournalEntryStatus.Posted, compensation.Status);
            Assert.AreEqual(posted.JournalEntryId,
                compensation.ReversalOfJournalEntryId);
            Assert.AreEqual(compensation.Id, source.ReversedByJournalEntryId);

            var originals = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == source.Id)
                .OrderBy(x => x.LineNumber).ToArrayAsync();
            var reversals = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == compensation.Id)
                .OrderBy(x => x.LineNumber).ToArrayAsync();
            Assert.AreEqual(2, reversals.Length);
            for (var i = 0; i < originals.Length; i++)
            {
                Assert.AreEqual(originals[i].AccountId, reversals[i].AccountId);
                Assert.AreEqual(originals[i].Credit, reversals[i].Debit);
                Assert.AreEqual(originals[i].Debit, reversals[i].Credit);
            }
        }

        using var detailResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}");
        Assert.AreEqual(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content
            .ReadFromJsonAsync<BankStatementResponse>();
        Assert.IsNotNull(detail);
        Assert.AreEqual(corrected.ReversalJournalEntryId,
            detail.Lines.Single().CorrectionReversalJournalEntryId);
        Assert.AreEqual(posted.JournalEntryId,
            detail.Lines.Single().FeeJournalEntryId);

        using var reportResponse = await client.GetAsync(
            $"/api/v1/banking/statements/{statement.Id}/reconciliation");
        var report = await reportResponse.Content
            .ReadFromJsonAsync<BankReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(0, report.PostedFeeLines);
        Assert.AreEqual(1, report.UnmatchedLines);
        Assert.IsFalse(report.FullyReconciled);
    }

    [TestMethod]
    public async Task ControlledFeeCorrectionReplaysIdenticallyAndRejectsChangedTerms()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "CORRECT-FEE-02", -7m);
        var line = await BankLineIdAsync(statement.Id);
        using var create = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, line),
            new PostBankFeeRequest(expenseA, -7m,
                "Quarterly account statement fee"));
        var posted = await create.Content
            .ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(posted);

        var url = CorrectionUrl(statement.Id, line);
        var request = new CorrectBankPostingRequest(
            "BankFee", posted.JournalEntryId, PostingDate,
            "Correction justified by the bank reconciliation");
        using var first = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        var initial = await first.Content
            .ReadFromJsonAsync<BankPostingCorrectionResponse>();
        Assert.IsNotNull(initial);

        using var repeated = await client.PostAsJsonAsync(url, request);
        Assert.AreEqual(HttpStatusCode.OK, repeated.StatusCode);
        var again = await repeated.Content
            .ReadFromJsonAsync<BankPostingCorrectionResponse>();
        Assert.IsNotNull(again);
        Assert.IsTrue(again.AlreadyCorrected);
        Assert.AreEqual(initial.ReversalJournalEntryId,
            again.ReversalJournalEntryId);

        using var changed = await client.PostAsJsonAsync(
            url, request with { Reason = "Different correction reason" });
        Assert.AreEqual(HttpStatusCode.Conflict, changed.StatusCode);

        using var originalFee = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, line),
            new PostBankFeeRequest(expenseA, -7m,
                "Quarterly account statement fee"));
        Assert.AreEqual(HttpStatusCode.Conflict, originalFee.StatusCode);
    }

    [TestMethod]
    public async Task ControlledForeignSettlementCompensationPreservesNativeStatement()
    {
        var setup = await SetupFxBankAsync();
        using var client = Client(userA, organizationA);
        var statement = await ImportFxStatementAsync(
            client, setup.Bank.Id, 120m);
        var line = await BankLineIdAsync(statement.Id);
        using var settlement = await client.PostAsJsonAsync(
            FxUrl(statement.Id, line), FxRequest(setup, 120m, 95m));
        Assert.AreEqual(HttpStatusCode.OK, settlement.StatusCode);
        var posted = await settlement.Content
            .ReadFromJsonAsync<ForeignBankSettlementResponse>();
        Assert.IsNotNull(posted);

        using var response = await client.PostAsJsonAsync(
            CorrectionUrl(statement.Id, line),
            new CorrectBankPostingRequest(
                "ForeignSettlement", posted.JournalEntryId, PostingDate,
                "Incorrect receivable carrying value in settlement"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var corrected = await response.Content
            .ReadFromJsonAsync<BankPostingCorrectionResponse>();
        Assert.IsNotNull(corrected);
        Assert.AreEqual(posted.JournalEntryId, corrected.OriginalJournalEntryId);

        await using var db = CreateDb();
        var original = await db.JournalEntries.AsNoTracking()
            .SingleAsync(x => x.Id == posted.JournalEntryId);
        var reversal = await db.JournalEntries.AsNoTracking()
            .SingleAsync(x => x.Id == corrected.ReversalJournalEntryId);
        Assert.AreEqual(JournalEntryStatus.Reversed, original.Status);
        Assert.AreEqual(JournalEntryStatus.Posted, reversal.Status);
        Assert.AreEqual(original.Id, reversal.ReversalOfJournalEntryId);
        var originalLines = await db.JournalLines.AsNoTracking()
            .Where(x => x.JournalEntryId == original.Id).ToArrayAsync();
        var reversalLines = await db.JournalLines.AsNoTracking()
            .Where(x => x.JournalEntryId == reversal.Id).ToArrayAsync();
        Assert.AreEqual(3, originalLines.Length);
        Assert.AreEqual(3, reversalLines.Length);
        Assert.AreEqual(originalLines.Sum(x => x.Credit),
            reversalLines.Sum(x => x.Debit));
    }

    [TestMethod]
    public async Task ForeignSettlementCannotBeCorrectedOnceCutoffWasRevalued()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        using (var revalued = await client.PostAsJsonAsync(
            ValuationUrl(setup.StatementId),
            RevaluationRequest(setup.RateId, 120m, 100m)))
            Assert.AreEqual(HttpStatusCode.OK, revalued.StatusCode);

        var line = await BankLineIdAsync(setup.StatementId);
        Guid original;
        await using (var db = CreateDb())
        {
            original = (await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.OrganizationId == organizationA &&
                    x.ExternalReference ==
                        $"bank-fx-settlement:v1:{line:N}")).Id;
        }
        using var forbidden = await client.PostAsJsonAsync(
            CorrectionUrl(setup.StatementId, line),
            new CorrectBankPostingRequest(
                "ForeignSettlement", original, PostingDate,
                "Historical FX posting is already in a validated cutoff"));
        Assert.AreEqual(HttpStatusCode.Conflict, forbidden.StatusCode);
    }

    [TestMethod]
    public async Task BankCorrectionRejectsCrossTenantAndUnrelatedJournal()
    {
        using var owner = Client(userA, organizationA);
        using var foreign = Client(userB, organizationB);
        var bank = await CreateTestBankAccountAsync(owner);
        var statement = await ImportBankFeeStatementAsync(
            owner, bank.Id, "CORRECT-SECURITY", -3m);
        var line = await BankLineIdAsync(statement.Id);
        using var create = await owner.PostAsJsonAsync(
            FeeUrl(statement.Id, line),
            new PostBankFeeRequest(expenseA, -3m,
                "Bank maintenance correction original"));
        var fee = await create.Content.ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(fee);

        var url = CorrectionUrl(statement.Id, line);
        var request = new CorrectBankPostingRequest(
            "BankFee", fee.JournalEntryId, PostingDate,
            "Review of original bank fee accounting");
        using (var denied = await foreign.PostAsJsonAsync(url, request))
            Assert.AreEqual(HttpStatusCode.NotFound, denied.StatusCode);

        using var wrongJournal = await owner.PostAsJsonAsync(
            url, request with { OriginalJournalEntryId = Guid.NewGuid() });
        Assert.AreEqual(HttpStatusCode.Conflict, wrongJournal.StatusCode);

        using var wrongKind = await owner.PostAsJsonAsync(
            url, request with { SourceKind = "ForeignSettlement" });
        Assert.AreEqual(HttpStatusCode.Conflict, wrongKind.StatusCode);
    }

    [TestMethod]
    public async Task ControlledBankCorrectionRejectsClosedPostingPeriod()
    {
        using var client = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(client);
        var statement = await ImportBankFeeStatementAsync(
            client, bank.Id, "CORRECT-LOCK", -8m);
        var line = await BankLineIdAsync(statement.Id);
        using var create = await client.PostAsJsonAsync(
            FeeUrl(statement.Id, line),
            new PostBankFeeRequest(expenseA, -8m,
                "Original account maintenance fee"));
        var posted = await create.Content.ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(posted);

        var calendar = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(client, calendar.YearId, calendar.PeriodId);
        using var denied = await client.PostAsJsonAsync(
            CorrectionUrl(statement.Id, line),
            new CorrectBankPostingRequest(
                "BankFee", posted.JournalEntryId, PostingDate,
                "Correction forbidden inside a closed accounting period"));
        Assert.AreEqual(HttpStatusCode.Conflict, denied.StatusCode);
    }

    [TestMethod]
    public async Task ConcurrentCorrectionSubmissionsCreateOneCompensatingJournal()
    {
        using var owner = Client(userA, organizationA);
        using var other = Client(userA, organizationA);
        var bank = await CreateTestBankAccountAsync(owner);
        var statement = await ImportBankFeeStatementAsync(
            owner, bank.Id, "CORRECT-CONCURRENT", -12m);
        var line = await BankLineIdAsync(statement.Id);
        using var create = await owner.PostAsJsonAsync(
            FeeUrl(statement.Id, line),
            new PostBankFeeRequest(expenseA, -12m,
                "Monthly account service charge"));
        var posted = await create.Content.ReadFromJsonAsync<BankFeePostingResponse>();
        Assert.IsNotNull(posted);

        var req = new CorrectBankPostingRequest(
            "BankFee", posted.JournalEntryId, PostingDate,
            "Correction of a wrongly coded expense");
        var url = CorrectionUrl(statement.Id, line);
        var requests = await Task.WhenAll(
            owner.PostAsJsonAsync(url, req),
            other.PostAsJsonAsync(url, req));
        try
        {
            Assert.IsTrue(requests.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join("; ", await Task.WhenAll(requests.Select(x =>
                    x.Content.ReadAsStringAsync()))));
            var results = await Task.WhenAll(requests.Select(x =>
                x.Content.ReadFromJsonAsync<BankPostingCorrectionResponse>()));
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[1]);
            Assert.AreEqual(results[0].ReversalJournalEntryId,
                results[1].ReversalJournalEntryId);
            Assert.AreNotEqual(results[0].AlreadyCorrected,
                results[1].AlreadyCorrected);
        }
        finally
        {
            foreach (var response in requests) response.Dispose();
        }
    }

    private static string CorrectionUrl(Guid statementId, Guid lineId) =>
        $"/api/v1/banking/statements/{statementId}/lines/{lineId}/correct-posting";
}
