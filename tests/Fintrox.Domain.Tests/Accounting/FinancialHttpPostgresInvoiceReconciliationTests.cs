using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task SourceReconciliationFlagsIssuedInvoiceAndReceivedPurchaseWithoutGL()
    {
        await SeedInvoiceAndPurchaseWithoutJournalsAsync();
        var fiscal = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/source-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content
            .ReadFromJsonAsync<FinancialSourceReconciliationResponse>();
        Assert.IsNotNull(report);
        Assert.AreEqual(2, report.SourceDocuments);
        Assert.AreEqual(2, report.IssuesCount);
        Assert.IsTrue(report.Issues.Any(x =>
            x.SourceType == "SalesInvoice" && x.Code == "MissingPosting"));
        Assert.IsTrue(report.Issues.Any(x =>
            x.SourceType == "PurchaseDocument" && x.Code == "MissingPosting"));
        Assert.IsFalse(report.SalesPostingIntegrityVerified);
        Assert.IsFalse(report.PurchasesPostingIntegrityVerified);
        Assert.IsTrue(report.PaymentsPostingIntegrityVerified);
    }

    private async Task SeedInvoiceAndPurchaseWithoutJournalsAsync()
    {
        await using var db = CreateDb();
        var now = DateTimeOffset.UtcNow;
        var currency = Currency.Create(
            organizationA, "EUR", "Euro", null, 2, true, now);
        var party = Counterparty.Create(
            organizationA, "SRC001", "Counterparty for financial source",
            null, "BG", null, null, true, true, 30,
            null, null, null, null, null, null, null, null, null, now);
        db.Currencies.Add(currency);
        db.Counterparties.Add(party);

        var invoice = SalesInvoice.CreateDraft(
            organizationA, party.Id, PostingDate, PostingDate,
            currency.Id, party.Name, null, null, null, "BG",
            null, null, null, null, "EUR", null, now);
        invoice.SetTotals(100m, 0m, 100m, now);
        invoice.Issue("INV-SRC-01", currency.Id, "EUR", "EUR",
            party.Name, null, null, null, "BG",
            null, null, null, null, 1m, now);

        var purchase = PurchaseDocument.CreateDraft(
            organizationA, PurchaseDocumentType.Invoice,
            party.Id, "SUP-SRC-01", PostingDate, PostingDate,
            currency.Id, party.Name, null, null, null, "BG",
            null, null, null, null, "EUR", null, now);
        purchase.SetTotals(70m, 0m, 0m, 0m, 70m, now);
        purchase.Receive("PUR-SRC-01", "SUP-SRC-01",
            currency.Id, "EUR", "EUR", party.Name,
            null, null, null, "BG",
            null, null, null, null, 1m, now);

        db.SalesInvoices.Add(invoice);
        db.PurchaseDocuments.Add(purchase);
        await db.SaveChangesAsync();
    }
}
