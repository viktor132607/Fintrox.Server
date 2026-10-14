using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>Real authenticated HTTP / disposable migrated PostgreSQL cases.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task AmendedForeignBankRateReversesV1AndPostsSeparateV2Loss()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        await PostTestValuationAsync(client, setup.StatementId, setup.RateId);
        var originalEvidence = await ReadFxCloseAsync(client, setup.StatementId);
        Assert.AreEqual(1, originalEvidence.VersionNumber);
        Assert.IsTrue(originalEvidence.InternallyReconciled);

        var newRate = await CreateEndRateAsync(1.5m);
        var request = AmendRateRequest(newRate, 1, null);
        var originalJournal = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        request = request with { ExpectedCurrentJournalEntryId = originalJournal };

        using var amended = await client.PostAsJsonAsync(
            AmendUrl(setup.StatementId), request);
        Assert.AreEqual(HttpStatusCode.OK, amended.StatusCode,
            await amended.Content.ReadAsStringAsync());
        var version = await amended.Content
            .ReadFromJsonAsync<ForeignBankRevaluationAmendmentResponse>();
        Assert.IsNotNull(version);
        Assert.AreEqual(2, version.VersionNumber);
        Assert.AreEqual(originalJournal, version.PreviousJournalEntryId);
        Assert.IsNotNull(version.ReversalJournalEntryId);
        Assert.IsNotNull(version.ReplacementJournalEntryId);
        Assert.AreEqual(120m, version.PreviousTargetBase);
        Assert.AreEqual(80m, version.ReplacementTargetBase);
        Assert.AreEqual(-20m, version.ReplacementAdjustmentBase);
        Assert.IsFalse(version.AlreadyAmended);

        await using (var db = CreateDb())
        {
            var old = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == originalJournal);
            var inverse = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == version.ReversalJournalEntryId);
            var updated = await db.JournalEntries.AsNoTracking()
                .SingleAsync(x => x.Id == version.ReplacementJournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Reversed, old.Status);
            Assert.AreEqual(JournalEntryStatus.Posted, inverse.Status);
            Assert.AreEqual(old.Id, inverse.ReversalOfJournalEntryId);
            Assert.AreEqual(inverse.Id, old.ReversedByJournalEntryId);
            Assert.AreEqual(JournalEntryStatus.Posted, updated.Status);
            Assert.AreEqual(
                $"bank-fx-revaluation:v2:{setup.BankId:N}:20260331",
                updated.ExternalReference);
            var correction = await db.JournalLines.AsNoTracking()
                .Where(x => x.JournalEntryId == updated.Id).ToArrayAsync();
            Assert.AreEqual(20m, correction.Single(x =>
                x.AccountId == assetA).Credit);
            Assert.AreEqual(20m, correction.Single(x =>
                x.AccountId == expenseA).Debit);
        }

        var current = await ReadFxCloseAsync(client, setup.StatementId);
        Assert.AreEqual(2, current.VersionNumber);
        Assert.IsTrue(current.InternallyReconciled,
            string.Join(", ", current.Issues));
        Assert.AreEqual(80m, current.ActualPostedLedgerBase);
        Assert.AreEqual(80m, current.RevaluedTargetBase);
        Assert.AreEqual(0m, current.BaseDifference);
        Assert.IsFalse(current.Prepared);
        Assert.IsFalse(current.Approved);

        using var replay = await client.PostAsJsonAsync(
            AmendUrl(setup.StatementId), request);
        Assert.AreEqual(HttpStatusCode.OK, replay.StatusCode);
        var identical = await replay.Content
            .ReadFromJsonAsync<ForeignBankRevaluationAmendmentResponse>();
        Assert.IsNotNull(identical);
        Assert.IsTrue(identical.AlreadyAmended);
        Assert.AreEqual(version.ReplacementJournalEntryId,
            identical.ReplacementJournalEntryId);
    }

    [TestMethod]
    public async Task AmendmentInvalidatesV1ApprovalAndRequiresNewTwoPersonReview()
    {
        var setup = await SetupValuationAsync(1m);
        using var maker = Client(userA, organizationA);
        await PostTestValuationAsync(maker, setup.StatementId, setup.RateId);
        using (var prepared = await maker.PostAsJsonAsync(
            PrepareUrl(setup.StatementId),
            new PrepareForeignBankCloseRequest(
                "The initial FX bank cutoff has been fully prepared")))
            Assert.AreEqual(HttpStatusCode.OK, prepared.StatusCode);
        var checkerId = await SeedOtherBankReviewerAsync();
        using var checker = Client(checkerId, organizationA);
        using (var approved = await checker.PostAsJsonAsync(
            ApproveUrl(setup.StatementId),
            new ApproveForeignBankCloseRequest(
                "Independent audit of the original FX bank rate")))
            Assert.AreEqual(HttpStatusCode.OK, approved.StatusCode);
        Assert.IsTrue((await ReadFxCloseAsync(maker, setup.StatementId)).Approved);

        var revised = await CreateEndRateAsync(1.5m);
        var prior = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        using (var amend = await maker.PostAsJsonAsync(
            AmendUrl(setup.StatementId),
            AmendRateRequest(revised, 1, prior)))
            Assert.AreEqual(HttpStatusCode.OK, amend.StatusCode,
                await amend.Content.ReadAsStringAsync());

        var beforeReview = await ReadFxCloseAsync(maker, setup.StatementId);
        Assert.AreEqual(2, beforeReview.VersionNumber);
        Assert.IsTrue(beforeReview.InternallyReconciled);
        Assert.IsFalse(beforeReview.Prepared);
        Assert.IsFalse(beforeReview.Approved);
        using (var noSelfApproval = await maker.PostAsJsonAsync(
            ApproveUrl(setup.StatementId),
            new ApproveForeignBankCloseRequest(
                "Invalid attempt to reuse historic approval")))
            Assert.AreEqual(HttpStatusCode.Conflict, noSelfApproval.StatusCode);

        using (var preparedV2 = await maker.PostAsJsonAsync(
            PrepareUrl(setup.StatementId),
            new PrepareForeignBankCloseRequest(
                "Prepared replacement valuation and its new audit chain")))
        {
            Assert.AreEqual(HttpStatusCode.OK, preparedV2.StatusCode,
                await preparedV2.Content.ReadAsStringAsync());
            var recorded = await preparedV2.Content
                .ReadFromJsonAsync<ForeignBankCloseDecisionResponse>();
            Assert.IsNotNull(recorded);
            Assert.AreEqual(2, recorded.VersionNumber);
        }
        using (var self = await maker.PostAsJsonAsync(
            ApproveUrl(setup.StatementId),
            new ApproveForeignBankCloseRequest(
                "Maker must never approve own restated FX close")))
            Assert.AreEqual(HttpStatusCode.Conflict, self.StatusCode);
        using var approvedV2 = await checker.PostAsJsonAsync(
            ApproveUrl(setup.StatementId),
            new ApproveForeignBankCloseRequest(
                "Independently checked the new approved FX rate"));
        Assert.AreEqual(HttpStatusCode.OK, approvedV2.StatusCode,
            await approvedV2.Content.ReadAsStringAsync());
        var decision = await approvedV2.Content
            .ReadFromJsonAsync<ForeignBankCloseDecisionResponse>();
        Assert.IsNotNull(decision);
        Assert.AreEqual(2, decision.VersionNumber);
        Assert.IsTrue(decision.Approved);
        Assert.IsTrue((await ReadFxCloseAsync(maker, setup.StatementId)).Approved);
    }

    [TestMethod]
    public async Task AmendmentRejectsWrongVersionRateAndBaseExpectations()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        await PostTestValuationAsync(client, setup.StatementId, setup.RateId);
        var old = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        var newRate = await CreateEndRateAsync(1.5m);
        var url = AmendUrl(setup.StatementId);
        using (var wrongVersion = await client.PostAsJsonAsync(
            url, AmendRateRequest(newRate, 2, old)))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongVersion.StatusCode);
        using (var wrongJournal = await client.PostAsJsonAsync(
            url, AmendRateRequest(newRate, 1, Guid.NewGuid())))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongJournal.StatusCode);
        using (var wrongBalance = await client.PostAsJsonAsync(
            url, AmendRateRequest(newRate, 1, old) with
                { ExpectedLedgerBaseBalance = 90m }))
            Assert.AreEqual(HttpStatusCode.Conflict, wrongBalance.StatusCode);
        using var wrongFx = await client.PostAsJsonAsync(
            url, AmendRateRequest(Guid.NewGuid(), 1, old));
        Assert.AreEqual(HttpStatusCode.Conflict, wrongFx.StatusCode);
    }

    [TestMethod]
    public async Task AmendmentCannotRestateClosedFiscalPeriod()
    {
        var setup = await SetupValuationAsync(1m);
        using var client = Client(userA, organizationA);
        await PostTestValuationAsync(client, setup.StatementId, setup.RateId);
        var old = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        var newRate = await CreateEndRateAsync(1.5m);
        var cal = await MarchCalendarAsync(organizationA);
        await CloseMarchAsync(client, cal.YearId, cal.PeriodId);
        using var result = await client.PostAsJsonAsync(
            AmendUrl(setup.StatementId), AmendRateRequest(newRate, 1, old));
        Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
    }

    [TestMethod]
    public async Task AmendmentIsTenantScoped()
    {
        var setup = await SetupValuationAsync(1m);
        using var owner = Client(userA, organizationA);
        using var other = Client(userB, organizationB);
        await PostTestValuationAsync(owner, setup.StatementId, setup.RateId);
        var newRate = await CreateEndRateAsync(1.5m);
        var old = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        using var denied = await other.PostAsJsonAsync(
            AmendUrl(setup.StatementId), AmendRateRequest(newRate, 1, old));
        Assert.AreEqual(HttpStatusCode.NotFound, denied.StatusCode);
    }

    [TestMethod]
    public async Task ConcurrentAmendmentsProduceOneVersionAndOneReplay()
    {
        var setup = await SetupValuationAsync(1m);
        using var one = Client(userA, organizationA);
        using var two = Client(userA, organizationA);
        await PostTestValuationAsync(one, setup.StatementId, setup.RateId);
        var rate = await CreateEndRateAsync(1.5m);
        var old = await LookupOriginalRevaluationJournalAsync(setup.StatementId);
        var request = AmendRateRequest(rate, 1, old);
        var url = AmendUrl(setup.StatementId);
        var responses = await Task.WhenAll(
            one.PostAsJsonAsync(url, request),
            two.PostAsJsonAsync(url, request));
        try
        {
            Assert.IsTrue(responses.All(x => x.StatusCode == HttpStatusCode.OK),
                string.Join(";", await Task.WhenAll(responses.Select(x =>
                    x.Content.ReadAsStringAsync()))));
            var a = await responses[0].Content
                .ReadFromJsonAsync<ForeignBankRevaluationAmendmentResponse>();
            var b = await responses[1].Content
                .ReadFromJsonAsync<ForeignBankRevaluationAmendmentResponse>();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreEqual(a.ReplacementJournalEntryId, b.ReplacementJournalEntryId);
            Assert.AreNotEqual(a.AlreadyAmended, b.AlreadyAmended);
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
    }

    private static string AmendUrl(Guid statementId) =>
        $"/api/v1/banking/statements/{statementId}/amend-foreign-revaluation";

    private AmendForeignBankRevaluationRequest AmendRateRequest(
        Guid rateId, int version, Guid? priorJournal) =>
        new(version, priorJournal, rateId, revenueA, expenseA,
            120m, 100m, "Approved correction to original cutoff conversion rate");

    private async Task<Guid?> LookupOriginalRevaluationJournalAsync(Guid statementId)
    {
        await using var db = CreateDb();
        return await db.Database.SqlQueryRaw<Guid?>(
            "SELECT journal_entry_id AS \"Value\" FROM banking.bank_fx_revaluations " +
            "WHERE organization_id={0} AND statement_id={1}",
            organizationA, statementId).SingleAsync();
    }

    private static async Task<ForeignBankCloseEvidenceResponse> ReadFxCloseAsync(
        HttpClient client, Guid statementId)
    {
        using var response = await client.GetAsync(EvidenceUrl(statementId));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var evidence = await response.Content
            .ReadFromJsonAsync<ForeignBankCloseEvidenceResponse>();
        Assert.IsNotNull(evidence);
        return evidence;
    }
}
