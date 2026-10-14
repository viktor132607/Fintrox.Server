using System.Net;
using System.Net.Http.Json;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Partners;
using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

public sealed partial class FinancialHttpPostgresRegressionTests
{
    [TestMethod]
    public async Task OpenItemReconciliationMatchesARAndAPAfterPartialPayments()
    {
        var values = await SeedOpenItemLedgerAsync(40m, 10m);
        var report = await GetOpenItemReportAsync();

        Assert.AreEqual(60m, report.ReceivablesOpenBase);
        Assert.AreEqual(50m, report.PayablesOpenBase);
        Assert.IsTrue(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
        Assert.AreEqual(0, report.Issues.Count,
            string.Join(" | ", report.Issues.Select(i => i.Code)));
        Assert.HasCount(2, report.OpenItems);
        Assert.HasCount(2, report.CounterpartyBalances);
        Assert.AreEqual(60m,
            report.CounterpartyBalances.Single(x => x.Type == "Receivables")
                .OutstandingBaseAmount);
        Assert.AreEqual(50m,
            report.CounterpartyBalances.Single(x => x.Type == "Payables")
                .OutstandingBaseAmount);
        var invoice = report.OpenItems.Single(x => x.DocumentId == values.InvoiceId);
        var purchase = report.OpenItems.Single(x => x.DocumentId == values.PurchaseId);
        Assert.AreEqual(40m, invoice.AllocatedDocumentAmount);
        Assert.AreEqual(60m, invoice.OutstandingDocumentAmount);
        Assert.AreEqual(values.ArAccountId, invoice.ControlAccountId);
        Assert.IsTrue(invoice.Overdue);
        Assert.AreEqual(10m, purchase.AllocatedDocumentAmount);
        Assert.AreEqual(50m, purchase.OutstandingDocumentAmount);
        Assert.AreEqual(values.ApAccountId, purchase.ControlAccountId);
        Assert.IsTrue(report.ControlAccounts.All(x => x.Balanced));
        Assert.IsFalse(report.HistoricalAsOfSnapshotAvailable);
        Assert.IsFalse(report.BankStatementsReconciled);
        Assert.IsFalse(report.FinancialSubsystemsFullyReconciled);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsManualControlAccountDifference()
    {
        var values = await SeedOpenItemLedgerAsync(40m, 10m);
        await SeedOpenItemJournalAsync(
            $"manual-control:{Guid.NewGuid():N}",
            "Additional AR journal",
            (values.ArAccountId, 9m, 0m, "Manual debit to receivables"),
            (equityA, 0m, 9m, "Manual balancing credit"));
        var report = await GetOpenItemReportAsync();

        Assert.AreEqual(60m, report.ReceivablesOpenBase);
        Assert.IsFalse(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
        Assert.IsTrue(report.Issues.Any(i =>
            i.Code == "ControlAccountDifference" &&
            i.SourceType == "Receivables"));
        var ar = report.ControlAccounts.Single(x =>
            x.AccountId == values.ArAccountId && x.Type == "Receivables");
        Assert.AreEqual(69m, ar.ActualLedgerBalanceBase);
        Assert.AreEqual(60m, ar.ExpectedOpenItemsBase);
        Assert.AreEqual(9m, ar.DifferenceBase);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsOverallocatedCustomerDocument()
    {
        await SeedOpenItemLedgerAsync(120m, 10m);
        var report = await GetOpenItemReportAsync();

        Assert.IsTrue(report.Issues.Any(x =>
            x.Code == "OverallocatedDocument" &&
            x.SourceType == "Receivables"));
        Assert.IsFalse(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
        Assert.AreEqual(-20m, report.ReceivablesOpenBase);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsWrongPaymentCounterparty()
    {
        await SeedOpenItemLedgerAsync(
            40m, 10m, wrongCustomerPaymentParty: true);
        var report = await GetOpenItemReportAsync();

        Assert.IsTrue(report.Issues.Any(issue =>
            issue.Code == "CounterpartyMismatch" &&
            issue.SourceType == "SalesInvoice"));
        Assert.IsFalse(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsPaymentHeaderAllocationMismatch()
    {
        await SeedOpenItemLedgerAsync(
            40m, 10m, declaredIncomingAllocatedBase: 20m);
        var report = await GetOpenItemReportAsync();

        Assert.IsTrue(report.Issues.Any(issue =>
            issue.SourceType == "Receivables" &&
            issue.Code == "PaymentAllocationMismatch"));
        Assert.IsFalse(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsUnmatchedPayablesLedgerBalance()
    {
        var values = await SeedOpenItemLedgerAsync(40m, 10m);
        await SeedOpenItemJournalAsync(
            $"manual-control:{Guid.NewGuid():N}",
            "Extra supplier payable",
            (expenseA, 5m, 0m, "Unlinked expense"),
            (values.ApAccountId, 0m, 5m, "Unlinked payable"));
        var report = await GetOpenItemReportAsync();
        Assert.IsTrue(report.AccountsReceivableReconciled);
        Assert.IsFalse(report.AccountsPayableReconciled);
        var ap = report.ControlAccounts.Single(row =>
            row.AccountId == values.ApAccountId && row.Type == "Payables");
        Assert.AreEqual(55m, ap.ActualLedgerBalanceBase);
        Assert.AreEqual(50m, ap.ExpectedOpenItemsBase);
        Assert.AreEqual(5m, ap.DifferenceBase);
    }

    [TestMethod]
    public async Task OpenItemReconciliationRejectsUnconfiguredControlAccounts()
    {
        var report = await GetOpenItemReportAsync();

        Assert.IsFalse(report.AccountsReceivableReconciled);
        Assert.IsFalse(report.AccountsPayableReconciled);
        Assert.IsTrue(report.Issues.Count(x => x.Code == "MissingControlAccount") == 2);
        Assert.AreEqual(0m, report.ReceivablesOpenBase);
    }

    [TestMethod]
    public async Task OpenItemReconciliationUsesInvoiceRateForFxAllocation()
    {
        var values = await SeedOpenItemLedgerAsync(
            25m, 10m, customerInvoiceRate: 1.25m);
        var report = await GetOpenItemReportAsync();
        Assert.AreEqual(60m, report.ReceivablesOpenBase);
        Assert.IsTrue(report.AccountsReceivableReconciled);
        var invoice = report.OpenItems.Single(x =>
            x.DocumentId == values.InvoiceId);
        Assert.AreEqual(75m, invoice.OutstandingDocumentAmount);
        Assert.AreEqual(60m, invoice.OutstandingBaseAmount);
        Assert.AreEqual(25m, invoice.AllocatedDocumentAmount);
        Assert.AreEqual("90+", invoice.AgingBucket);
        Assert.IsTrue(invoice.DaysOverdue > 90);
        Assert.IsTrue(report.ControlAccounts.All(x => x.Balanced));
    }

    [TestMethod]
    public async Task OpenItemReconciliationProtectsTenantBoundary()
    {
        var fiscal = await MarchCalendarAsync(organizationA);
        using var other = Client(userB, organizationB);
        using var response = await other.GetAsync(
            $"/api/v1/accounting/fiscal-years/{fiscal.YearId}/open-item-reconciliation");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task OpenItemReconciliationDetectsDeclaredAllocationWithoutAnyRows()
    {
        // The shared migrated-Postgres fixture creates a confirmed payment
        // with an allocation header amount but no PaymentAllocation children.
        await SeedPaymentAsync(organizationA, confirmed: true, allocatedAmount: 25m);
        var report = await GetOpenItemReportAsync();

        Assert.IsTrue(report.Issues.Any(issue =>
            issue.Code == "PaymentAllocationMismatch" &&
            issue.SourceType == "Receivables"));
        Assert.IsFalse(report.AccountsReceivableReconciled);
    }

    [TestMethod]
    public async Task OpenItemReconciliationKeepsFullySettledDocumentOutOfOpenItems()
    {
        await SeedOpenItemLedgerAsync(100m, 60m);
        var report = await GetOpenItemReportAsync();

        Assert.AreEqual(0m, report.ReceivablesOpenBase);
        Assert.AreEqual(0m, report.PayablesOpenBase);
        Assert.AreEqual(0, report.OpenItems.Count);
        Assert.IsTrue(report.ControlAccounts.All(x => x.Balanced));
        Assert.IsTrue(report.AccountsReceivableReconciled);
        Assert.IsTrue(report.AccountsPayableReconciled);
    }

    private async Task<FinancialOpenItemReconciliationResponse>
        GetOpenItemReportAsync()
    {
        var year = await MarchCalendarAsync(organizationA);
        using var client = Client(userA, organizationA);
        using var response = await client.GetAsync(
            $"/api/v1/accounting/fiscal-years/{year.YearId}/open-item-reconciliation");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync());
        var report = await response.Content
            .ReadFromJsonAsync<FinancialOpenItemReconciliationResponse>();
        Assert.IsNotNull(report);
        return report;
    }

    private async Task<(Guid InvoiceId, Guid PurchaseId, Guid ArAccountId,
        Guid ApAccountId)> SeedOpenItemLedgerAsync(
            decimal customerSettlement, decimal supplierSettlement,
            decimal customerInvoiceRate = 1m,
            bool wrongCustomerPaymentParty = false,
            decimal? declaredIncomingAllocatedBase = null)
    {
        Guid invoiceId;
        Guid purchaseId;
        Guid arId;
        Guid apId;
        Guid incomePaymentId;
        Guid outgoingPaymentId;
        await using (var db = CreateDb())
        {
            var now = DateTimeOffset.UtcNow;
            var currency = Currency.Create(
                organizationA, "EUR", "Euro", null, 2, true, now);
            var invoiceCurrency = customerInvoiceRate == 1m
                ? currency
                : Currency.Create(
                    organizationA, "USD", "US Dollar", "$", 2, false, now);
            var paymentBaseAmount = decimal.Round(
                customerSettlement / customerInvoiceRate, 4,
                MidpointRounding.AwayFromZero);
            var customer = Counterparty.Create(
                organizationA, "AR-CASE", "Open item customer",
                null, "BG", null, null, true, false, 30,
                null, null, null, null, null, null, null, null, null, now);
            var supplier = Counterparty.Create(
                organizationA, "AP-CASE", "Open item supplier",
                null, "BG", null, null, false, true, 30,
                null, null, null, null, null, null, null, null, null, now);
            var ar = Account.Create(
                organizationA, "411", "Accounts receivable",
                AccountType.Asset, null, true, true, now);
            var ap = Account.Create(
                organizationA, "401", "Accounts payable",
                AccountType.Liability, null, true, true, now);
            arId = ar.Id;
            apId = ap.Id;
            db.Currencies.Add(currency);
            if (invoiceCurrency.Id != currency.Id)
                db.Currencies.Add(invoiceCurrency);
            db.Counterparties.AddRange(customer, supplier);
            db.Accounts.AddRange(ar, ap);
            db.AutoPostingRules.AddRange(
                AutoPostingRule.Create(
                    organizationA, PostingComponent.AccountsReceivable,
                    PostingRuleMatchKind.Default, "*", arId, now),
                AutoPostingRule.Create(
                    organizationA, PostingComponent.AccountsPayable,
                    PostingRuleMatchKind.Default, "*", apId, now));

            var invoice = SalesInvoice.CreateDraft(
                organizationA, customer.Id, PostingDate,
                new DateOnly(2026, 4, 14), invoiceCurrency.Id, customer.Name,
                null, null, null, "BG", null, null, null, null,
                invoiceCurrency.Code, null, now);
            invoice.SetTotals(100m, 0m, 100m, now);
            invoice.Issue("AR-SRC-01", currency.Id, "EUR", invoiceCurrency.Code,
                customer.Name, null, null, null, "BG",
                null, null, null, null, customerInvoiceRate, now);
            invoiceId = invoice.Id;
            db.SalesInvoices.Add(invoice);

            var purchase = PurchaseDocument.CreateDraft(
                organizationA, PurchaseDocumentType.Invoice,
                supplier.Id, "AP-SUP-01", PostingDate,
                new DateOnly(2026, 4, 15), currency.Id, supplier.Name,
                null, null, null, "BG", null, null, null, null,
                "EUR", null, now);
            purchase.SetTotals(60m, 0m, 0m, 0m, 60m, now);
            purchase.Receive("AP-INT-01", "AP-SUP-01",
                currency.Id, "EUR", "EUR", supplier.Name,
                null, null, null, "BG",
                null, null, null, null, 1m, now);
            purchaseId = purchase.Id;
            db.PurchaseDocuments.Add(purchase);

            var incoming = Payment.CreateDraft(
                organizationA, PaymentDirection.Incoming,
                wrongCustomerPaymentParty ? supplier.Id : customer.Id,
                PostingDate, PaymentMethod.BankTransfer,
                currency.Id, "EUR", paymentBaseAmount, customer.Name,
                null, null, null, null, now);
            incoming.SetAllocatedAmount(
                declaredIncomingAllocatedBase ?? paymentBaseAmount, now);
            incoming.Confirm("IN-OPEN-001", currency.Id, "EUR",
                "EUR", customer.Name, null, null, 1m, now);
            incomePaymentId = incoming.Id;
            db.Payments.Add(incoming);
            db.PaymentAllocations.Add(PaymentAllocation.Create(
                organizationA, incoming.Id, 1,
                PaymentAllocationTargetType.SalesInvoice,
                invoice.Id, null, invoice.Number!, invoiceCurrency.Id,
                invoiceCurrency.Code, customerInvoiceRate,
                customerSettlement, paymentBaseAmount, now));

            var outgoing = Payment.CreateDraft(
                organizationA, PaymentDirection.Outgoing, supplier.Id,
                PostingDate, PaymentMethod.BankTransfer,
                currency.Id, "EUR", supplierSettlement, supplier.Name,
                null, null, null, null, now);
            outgoing.SetAllocatedAmount(supplierSettlement, now);
            outgoing.Confirm("OUT-OPEN-001", currency.Id, "EUR",
                "EUR", supplier.Name, null, null, 1m, now);
            outgoingPaymentId = outgoing.Id;
            db.Payments.Add(outgoing);
            db.PaymentAllocations.Add(PaymentAllocation.Create(
                organizationA, outgoing.Id, 1,
                PaymentAllocationTargetType.PurchaseDocument,
                null, purchase.Id, purchase.InternalNumber!, currency.Id,
                "EUR", 1m, supplierSettlement, supplierSettlement, now));
            await db.SaveChangesAsync();
        }

        await SeedOpenItemJournalAsync(
            $"sales-invoice:{invoiceId:N}", "Sales invoice issue",
            (arId, decimal.Round(100m / customerInvoiceRate, 4,
                MidpointRounding.AwayFromZero), 0m,
                "Accounts receivable - Open item customer"),
            (revenueA, 0m, decimal.Round(100m / customerInvoiceRate, 4,
                MidpointRounding.AwayFromZero), "Sales revenue"));
        await SeedOpenItemJournalAsync(
            $"purchase-document:{purchaseId:N}", "Purchase received",
            (expenseA, 60m, 0m, "Purchase expense"),
            (apId, 0m, 60m, "Accounts payable - Open item supplier"));
        await SeedOpenItemJournalAsync(
            $"payment:{incomePaymentId:N}", "Incoming payment",
            (assetA, decimal.Round(customerSettlement / customerInvoiceRate, 4,
                MidpointRounding.AwayFromZero), 0m, "Incoming BankTransfer"),
            (arId, 0m, decimal.Round(customerSettlement / customerInvoiceRate, 4,
                MidpointRounding.AwayFromZero), "Settle accounts receivable"));
        await SeedOpenItemJournalAsync(
            $"payment:{outgoingPaymentId:N}", "Outgoing payment",
            (apId, supplierSettlement, 0m, "Settle accounts payable"),
            (assetA, 0m, supplierSettlement, "Outgoing BankTransfer"));
        return (invoiceId, purchaseId, arId, apId);
    }

    private async Task SeedOpenItemJournalAsync(
        string externalReference,
        string description,
        params (Guid AccountId, decimal Debit, decimal Credit, string Text)[] lines)
    {
        await using var db = CreateDb();
        var period = await db.AccountingPeriods.AsNoTracking().SingleAsync(
            x => x.OrganizationId == organizationA &&
                x.StartDate == new DateOnly(2026, 3, 1));
        var now = DateTimeOffset.UtcNow;
        var entry = JournalEntry.CreateDraft(
            organizationA, period.Id, PostingDate, PostingDate,
            description,
            externalReference.StartsWith("manual-control:", StringComparison.Ordinal)
                ? JournalEntrySource.Manual
                : JournalEntrySource.System,
            externalReference, now);
        db.JournalEntries.Add(entry);
        var number = 1;
        foreach (var line in lines)
        {
            db.JournalLines.Add(JournalLine.Create(
                organizationA, entry.Id, number++,
                line.AccountId, line.Debit, line.Credit,
                line.Text, now));
        }
        await db.SaveChangesAsync();
        entry.Post($"OPEN-{Guid.NewGuid():N}"[..22], now);
        await db.SaveChangesAsync();
    }
}
