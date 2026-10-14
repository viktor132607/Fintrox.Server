using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

/// <summary>
/// Uses the existing disposable migrated PostgreSQL + authenticated HTTP fixture.
/// Each method gets its own database, so the financial test ledger remains isolated.
/// </summary>
public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task SourceReconciliationDoesNotClaimBankingVerification()
    {
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(0, report.IssuesCount);
        Assert.AreEqual(0, report.SourceDocuments);
        Assert.IsTrue(report.SalesPostingIntegrityVerified);
        Assert.IsTrue(report.PurchasesPostingIntegrityVerified);
        Assert.IsTrue(report.PaymentsPostingIntegrityVerified);
        Assert.IsFalse(report.BankingReconciliationAvailable);
        Assert.IsFalse(report.FinancialSubsystemsFullyReconciled);
    }

    [TestMethod]
    public async Task SourceReconciliationFlagsConfirmedPaymentWithoutSystemJournal()
    {
        await SeedPaymentAsync(organizationA, confirmed: true);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.SourceDocuments);
        Assert.AreEqual(0, report.SourceJournals);
        Assert.IsTrue(report.Issues.Any(i =>
            i.SourceType == "Payment" && i.Code == "MissingPosting"));
        Assert.IsFalse(report.PaymentsPostingIntegrityVerified);
    }

    [TestMethod]
    public async Task SourceReconciliationAcceptsConfirmedPaymentWithMatchingPostedGL()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(
            organizationA, $"payment:{paymentId:N}", 100m);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(0, report.IssuesCount,
            string.Join(" | ", report.Issues.Select(x => x.Code)));
        Assert.AreEqual(1, report.SourceDocuments);
        Assert.AreEqual(1, report.SourceJournals);
        Assert.IsTrue(report.PaymentsPostingIntegrityVerified);
        Assert.IsFalse(report.FinancialSubsystemsFullyReconciled);
    }

    [TestMethod]
    public async Task SourceReconciliationDetectsPaymentControlAmountMismatch()
    {
        var paymentId = await SeedPaymentAsync(organizationA, confirmed: true);
        await SeedSourceJournalAsync(
            organizationA, $"payment:{paymentId:N}", 90m);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsTrue(report.Issues.Any(x => x.Code == "ControlAmountMismatch"));
        Assert.IsFalse(report.PaymentsPostingIntegrityVerified);
    }

    [TestMethod]
    public async Task SourceReconciliationFlagsAllocationSummaryWithoutAllocationRows()
    {
        var paymentId = await SeedPaymentAsync(
            organizationA, confirmed: true, allocatedAmount: 25m);
        await SeedSourceJournalAsync(
            organizationA, $"payment:{paymentId:N}", 100m);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.IsTrue(report.Issues.Any(i =>
            i.SourceType == "Payment" && i.Code == "PaymentAllocationMismatch"));
        Assert.IsFalse(report.PaymentsPostingIntegrityVerified);
    }

    [TestMethod]
    public async Task SourceReconciliationDetectsOrphanSystemPaymentJournal()
    {
        await SeedSourceJournalAsync(
            organizationA, $"payment:{Guid.NewGuid():N}", 50m);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(0, report.SourceDocuments);
        Assert.AreEqual(1, report.SourceJournals);
        Assert.IsTrue(report.Issues.Any(x =>
            x.SourceType == "Payment" && x.Code == "OrphanPosting"));
    }

    [TestMethod]
    public async Task SourceReconciliationExcludesUnconfirmedPaymentAndOtherTenant()
    {
        await SeedPaymentAsync(organizationA, confirmed: false);
        await SeedPaymentAsync(organizationB, confirmed: true);
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(1, report.SourceDocuments);
        Assert.AreEqual(0, report.IssuesCount);
    }

    [TestMethod]
    public async Task SourceReconciliationRejectsOtherTenantCalendarAndAnonymousAccess()
    {
        var fiscal = await MarchCalendarAsync(organizationA);
        using var other = Client(userB, organizationB);
        using var crossTenant = await other.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.NotFound, crossTenant.StatusCode);
        using var anonymous = factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var rejected = await anonymous.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.Unauthorized, rejected.StatusCode);
    }

    private async Task<Guid> SeedPaymentAsync(
        Guid organizationId, bool confirmed, decimal allocatedAmount = 0m)
    {
        await using var db = CreateDb();
        var now = DateTimeOffset.UtcNow;
        var currency = Currency.Create(
            organizationId, "EUR", "Euro", null, 2, true, now);
        var party = Counterparty.Create(
            organizationId, "C001", "Customer C001", null, "BG",
            null, null, true, true, 30,
            null, null, null, null, null, null, null, null, null, now);
        db.Currencies.Add(currency);
        db.Counterparties.Add(party);
        var payment = Payment.CreateDraft(
            organizationId, PaymentDirection.Incoming,
            party.Id, PostingDate, PaymentMethod.BankTransfer,
            currency.Id, "EUR", 100m, party.Name,
            null, null, null, null, now);
        if (allocatedAmount > 0m)
            payment.SetAllocatedAmount(allocatedAmount, now);
        if (confirmed)
        {
            payment.Confirm(
                "PAY-TEST-001", currency.Id, "EUR", "EUR",
                party.Name, null, null, 1m, now);
        }

        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment.Id;
    }

    private async Task SeedSourceJournalAsync(
        Guid organizationId, string externalReference, decimal postedAmount)
    {
        await using var db = CreateDb();
        var period = await db.AccountingPeriods.AsNoTracking().SingleAsync(
            x => x.OrganizationId == organizationId &&
                 x.StartDate == new DateOnly(2026, 3, 1));
        var account = organizationId == organizationA ? assetA : assetB;
        var contra = organizationId == organizationA ? equityA : assetB;
        var now = DateTimeOffset.UtcNow;
        var entry = JournalEntry.CreateDraft(
            organizationId, period.Id, PostingDate, PostingDate,
            "System payment to reconcile", JournalEntrySource.System,
            externalReference, now);
        db.JournalEntries.Add(entry);
        db.JournalLines.AddRange(
            JournalLine.Create(organizationId, entry.Id, 1,
                account, postedAmount, 0m, "Incoming BankTransfer", now),
            JournalLine.Create(organizationId, entry.Id, 2,
                contra, 0m, postedAmount, "Payment counterpart", now));
        await db.SaveChangesAsync();
        entry.Post("TEST-SOURCE-001", now);
        await db.SaveChangesAsync();
    }
}
