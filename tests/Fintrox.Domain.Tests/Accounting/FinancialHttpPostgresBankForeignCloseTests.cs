using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>Real authenticated HTTP with migrated ephemeral PostgreSQL.</summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task ForeignBankBridgeMatchesNativeClosingAfterRevaluation()
    {
        var values = await SetupValuationAsync(1m);
        using var owner = Client(userA, organizationA);
        await PostTestValuationAsync(owner, values.StatementId, values.RateId);

        using var result = await owner.GetAsync(EvidenceUrl(values.StatementId));
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        var bridge = await result.Content
            .ReadFromJsonAsync<ForeignBankCloseEvidenceResponse>();
        Assert.IsNotNull(bridge);
        Assert.AreEqual(120m, bridge.NativeClosingBalance);
        Assert.AreEqual(1m, bridge.ApprovedCutoffRate);
        Assert.AreEqual(120m, bridge.RevaluedTargetBase);
        Assert.AreEqual(120m, bridge.ActualPostedLedgerBase);
        Assert.AreEqual(0m, bridge.BaseDifference);
        Assert.IsTrue(bridge.NativeStatementChainValid);
        Assert.IsTrue(bridge.AllNativeLinesPosted);
        Assert.IsTrue(bridge.RevaluationJournalValid);
        Assert.IsTrue(bridge.NoUnrecognizedLedgerEntries);
        Assert.IsTrue(bridge.InternallyReconciled);
        Assert.IsFalse(bridge.Approved);
        Assert.AreEqual(0, bridge.Issues.Count);
    }

    [TestMethod]
    public async Task ForeignBankCloseRequiresDistinctMakerAndChecker()
    {
        var values = await SetupValuationAsync(1m);
        using var maker = Client(userA, organizationA);
        await PostTestValuationAsync(maker, values.StatementId, values.RateId);
        var prepareUrl = PrepareUrl(values.StatementId);
        var approveUrl = ApproveUrl(values.StatementId);
        using var preparation = await maker.PostAsJsonAsync(
            prepareUrl, new PrepareForeignBankCloseRequest(
                "Prepared all foreign bank source evidence"));
        Assert.AreEqual(HttpStatusCode.OK, preparation.StatusCode,
            await preparation.Content.ReadAsStringAsync());
        var prepared = await preparation.Content
            .ReadFromJsonAsync<ForeignBankCloseDecisionResponse>();
        Assert.IsNotNull(prepared);
        Assert.IsFalse(prepared.Approved);

        using var self = await maker.PostAsJsonAsync(
            approveUrl, new ApproveForeignBankCloseRequest(
                "I want to approve my own reconciliation"));
        Assert.AreEqual(HttpStatusCode.Conflict, self.StatusCode);

        var reviewer = await SeedOtherBankReviewerAsync();
        using var checker = Client(reviewer, organizationA);
        using var approve = await checker.PostAsJsonAsync(
            approveUrl, new ApproveForeignBankCloseRequest(
                "Independently verified all documented amounts"));
        Assert.AreEqual(HttpStatusCode.OK, approve.StatusCode,
            await approve.Content.ReadAsStringAsync());
        var approved = await approve.Content
            .ReadFromJsonAsync<ForeignBankCloseDecisionResponse>();
        Assert.IsNotNull(approved);
        Assert.IsTrue(approved.Approved);
        Assert.AreEqual(userA, approved.PreparedByUserId);
        Assert.AreEqual(reviewer, approved.ApprovedByUserId);
        Assert.AreEqual(prepared.EvidenceSha256, approved.EvidenceSha256);

        using var reported = await maker.GetAsync(EvidenceUrl(values.StatementId));
        var report = await reported.Content
            .ReadFromJsonAsync<ForeignBankCloseEvidenceResponse>();
        Assert.IsNotNull(report);
        Assert.IsTrue(report.Prepared);
        Assert.IsTrue(report.Approved);

        using var retry = await checker.PostAsJsonAsync(
            approveUrl, new ApproveForeignBankCloseRequest(
                "Independently verified all documented amounts"));
        Assert.AreEqual(HttpStatusCode.OK, retry.StatusCode);
        var replay = await retry.Content
            .ReadFromJsonAsync<ForeignBankCloseDecisionResponse>();
        Assert.IsNotNull(replay);
        Assert.IsTrue(replay.AlreadyRecorded);
    }

    [TestMethod]
    public async Task ForeignBankCloseCannotBePreparedBeforeRevaluation()
    {
        var values = await SetupValuationAsync(1m);
        using var owner = Client(userA, organizationA);
        using var premature = await owner.PostAsJsonAsync(
            PrepareUrl(values.StatementId),
            new PrepareForeignBankCloseRequest(
                "No valuation has been posted to bank GL"));
        Assert.AreEqual(HttpStatusCode.Conflict, premature.StatusCode);
    }

    [TestMethod]
    public async Task ForeignBankCloseCannotBeApprovedAfterNewUnmatchedGlMovement()
    {
        var values = await SetupValuationAsync(1m);
        using var maker = Client(userA, organizationA);
        await PostTestValuationAsync(maker, values.StatementId, values.RateId);
        using (var prepared = await maker.PostAsJsonAsync(
            PrepareUrl(values.StatementId),
            new PrepareForeignBankCloseRequest(
                "Prepared before the unauthorized GL movement")))
            Assert.AreEqual(HttpStatusCode.OK, prepared.StatusCode);

        // A later posted manual entry changes the bank account's cumulative
        // balance and violates the immutable evidence fingerprint.
        await SeedOpenItemJournalAsync(
            $"unlinked-bank:{Guid.NewGuid():N}",
            "Unrecognized movement through foreign bank GL",
            (assetA, 5m, 0m, "Extra foreign bank debit"),
            (equityA, 0m, 5m, "Other side"));

        var reviewer = await SeedOtherBankReviewerAsync();
        using var checker = Client(reviewer, organizationA);
        using var rejected = await checker.PostAsJsonAsync(
            ApproveUrl(values.StatementId),
            new ApproveForeignBankCloseRequest(
                "Review finds differences in bank GL"));
        Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);

        using var read = await maker.GetAsync(EvidenceUrl(values.StatementId));
        var evidence = await read.Content
            .ReadFromJsonAsync<ForeignBankCloseEvidenceResponse>();
        Assert.IsNotNull(evidence);
        Assert.IsFalse(evidence.InternallyReconciled);
        Assert.IsFalse(evidence.Approved);
    }

    [TestMethod]
    public async Task ForeignBankCloseDoesNotExposeOtherTenantEvidence()
    {
        var values = await SetupValuationAsync(1m);
        using var foreign = Client(userB, organizationB);
        using var report = await foreign.GetAsync(EvidenceUrl(values.StatementId));
        Assert.AreEqual(HttpStatusCode.NotFound, report.StatusCode);
        using var prepare = await foreign.PostAsJsonAsync(
            PrepareUrl(values.StatementId),
            new PrepareForeignBankCloseRequest(
                "Unauthorized bank reconciliation request"));
        Assert.AreEqual(HttpStatusCode.NotFound, prepare.StatusCode);
        using var approve = await foreign.PostAsJsonAsync(
            ApproveUrl(values.StatementId),
            new ApproveForeignBankCloseRequest(
                "Unauthorized independent close approval"));
        Assert.AreEqual(HttpStatusCode.NotFound, approve.StatusCode);
    }

    private async Task<Guid> SeedOtherBankReviewerAsync()
    {
        var reviewer = Guid.NewGuid();
        await using var db = CreateDb();
        var now = DateTimeOffset.UtcNow;
        db.Users.Add(CreateUser(reviewer,
            $"fx-reviewer-{reviewer:N}@example.test", now));
        db.OrganizationMemberships.Add(OrganizationMembership.Create(
            organizationA, reviewer, OrganizationRole.Owner, now));
        await db.SaveChangesAsync();
        return reviewer;
    }

    private static string EvidenceUrl(Guid id) =>
        $"/api/v1/banking/statements/{id}/foreign-close-evidence";
    private static string PrepareUrl(Guid id) =>
        $"/api/v1/banking/statements/{id}/prepare-foreign-close";
    private static string ApproveUrl(Guid id) =>
        $"/api/v1/banking/statements/{id}/approve-foreign-close";

    private async Task PostTestValuationAsync(
        HttpClient client, Guid statementId, Guid rateId)
    {
        using var response = await client.PostAsJsonAsync(
            ValuationUrl(statementId), RevaluationRequest(rateId, 120m, 100m));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
    }
}
