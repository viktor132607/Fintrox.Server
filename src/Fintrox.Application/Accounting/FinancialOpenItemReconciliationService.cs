using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

/// <summary>
/// Current-state cumulative AR/AP open-item and GL control-account
/// reconciliation. Does not pretend to reconstruct past lifecycle versions.
/// </summary>
public sealed class FinancialOpenItemReconciliationService(
    IFinancialOpenItemRepository repository,
    IFiscalCalendarRepository calendar,
    IAutoPostingRuleRepository rules,
    ITransactionRunner transactions,
    ICurrentOrganization currentOrganization)
    : IFinancialOpenItemReconciliationService
{
    public async Task<FinancialOpenItemReconciliationResponse?> ReconcileAsync(
        Guid fiscalYearId, CancellationToken cancellationToken)
    {
        var orgId = currentOrganization.RequireOrganizationId();
        return await transactions.ExecuteAsync(async ct =>
        {
            await calendar.LockFiscalYearForCloseAsync(orgId, fiscalYearId, ct);
            var fiscalYear = await calendar.GetFiscalYearAsync(
                orgId, fiscalYearId, trackChanges: false, ct);
            if (fiscalYear is null) return null;

            // Lifetime cumulative balances, not just year-period movements:
            // open documents from earlier fiscal years may remain unsettled.
            var cutoff = fiscalYear.EndDate;
            var documents = await repository.GetDocumentsAsync(orgId, cutoff, ct);
            var payments = await repository.GetPaymentsAsync(orgId, cutoff, ct);
            var allocations = await repository.GetAllocationsAsync(orgId, cutoff, ct);
            var originalLines = await repository.GetOriginalControlLinesAsync(
                orgId, cutoff, ct);
            var configuredRules = await rules.ListAsync(
                orgId, includeInactive: true, ct);

            var issues = new List<FinancialOpenItemReconciliationIssue>();
            void AddIssue(string code, string type, Guid? documentId,
                Guid? paymentId, string description) =>
                issues.Add(new FinancialOpenItemReconciliationIssue(
                    code, type, documentId, paymentId, description));

            var arAccounts = configuredRules
                .Where(rule => rule.Component == PostingComponent.AccountsReceivable)
                .Select(rule => rule.AccountId).Distinct().ToHashSet();
            var apAccounts = configuredRules
                .Where(rule => rule.Component == PostingComponent.AccountsPayable)
                .Select(rule => rule.AccountId).Distinct().ToHashSet();

            if (arAccounts.Count == 0)
                AddIssue("MissingControlAccount", "Receivables", null, null,
                    "No accounts-receivable posting control account is configured.");
            if (apAccounts.Count == 0)
                AddIssue("MissingControlAccount", "Payables", null, null,
                    "No accounts-payable posting control account is configured.");

            var documentsByKey = documents.ToDictionary(
                document => (document.SourceType, document.Id));
            var allocatedByDocument = new Dictionary<(string Kind, Guid Id), decimal>();

            // Compare even a Confirmed payment without any allocation
            // rows; relying only on the allocation table would miss it.
            var paymentRowsById = allocations.GroupBy(row => row.PaymentId)
                .ToDictionary(group => group.Key, group => group.ToArray());
            foreach (var payment in payments.Where(p => p.Status == "Confirmed"))
            {
                var rows = paymentRowsById.TryGetValue(
                    payment.Id, out var found) ? found : [];
                var rowTotal = rows.Sum(row => row.PaymentAmount);
                if (rowTotal != payment.AllocatedAmount ||
                    rowTotal < 0m || rowTotal > payment.GrossAmount)
                {
                    var kind = payment.Direction == "Incoming"
                        ? "Receivables" : "Payables";
                    AddIssue("PaymentAllocationMismatch", kind, null,
                        payment.Id,
                        "Confirmed payment allocation rows do not match its declared payment-currency allocated amount.");
                }
            }

            foreach (var allocation in allocations)
            {
                // A Draft or Cancelled payment must not settle an open item.
                // Cancellation reversals are checked in ACCOUNTING-001F1.
                if (allocation.PaymentStatus != "Confirmed")
                    continue;

                var sales = allocation.SalesInvoiceId.HasValue &&
                            !allocation.PurchaseDocumentId.HasValue;
                var purchase = allocation.PurchaseDocumentId.HasValue &&
                               !allocation.SalesInvoiceId.HasValue;
                if (!sales && !purchase)
                {
                    AddIssue("InvalidAllocationTarget", "Payment", null,
                        allocation.PaymentId,
                        "The allocation must reference exactly one receivable or payable.");
                    continue;
                }

                var kind = sales ? "SalesInvoice" : "PurchaseDocument";
                var targetId = sales
                    ? allocation.SalesInvoiceId!.Value
                    : allocation.PurchaseDocumentId!.Value;
                var key = (kind, targetId);

                if (!documentsByKey.TryGetValue(key, out var target))
                {
                    AddIssue("MissingAllocatedDocument", kind, targetId,
                        allocation.PaymentId,
                        "A confirmed payment targets a document absent at this cutoff.");
                    continue;
                }

                if (allocation.PaymentCounterpartyId != target.CounterpartyId)
                    AddIssue("CounterpartyMismatch", kind, targetId,
                        allocation.PaymentId,
                        "Payment and allocated document have different counterparties.");

                if (allocation.Direction != (sales ? "Incoming" : "Outgoing"))
                    AddIssue("PaymentDirectionMismatch", kind, targetId,
                        allocation.PaymentId,
                        "The payment direction does not match the document type.");

                if (allocation.DocumentAmount <= 0m ||
                    allocation.PaymentAmount <= 0m)
                {
                    AddIssue("InvalidAllocationAmount", kind, targetId,
                        allocation.PaymentId, "Allocation amounts must be positive.");
                    continue;
                }

                if (target.Status == "Cancelled")
                    AddIssue("CancelledDocumentAllocated", kind, targetId,
                        allocation.PaymentId,
                        "A cancelled document has a confirmed payment allocation.");

                allocatedByDocument.TryGetValue(key, out var previous);
                allocatedByDocument[key] = previous + allocation.DocumentAmount;
            }

            var controlLinesByDocument = originalLines
                .GroupBy(line => (line.SourceType, line.DocumentId))
                .ToDictionary(group => group.Key, group => group.ToArray());

            var openItems = new List<FinancialOpenItemResponse>();
            var expectedByControl = new Dictionary<(string Type, Guid AccountId), decimal>();

            foreach (var document in documents)
            {
                if (document.Status == "Draft") continue;
                var receivable = document.SourceType == "SalesInvoice";
                var kind = receivable ? "Receivables" : "Payables";
                var shouldBeActive = receivable ? "Issued" : "Received";
                if (document.Status != shouldBeActive &&
                    document.Status != "Cancelled")
                {
                    AddIssue("UnsupportedDocumentStatus", kind, document.Id, null,
                        "The document has an unknown financial lifecycle status.");
                    continue;
                }

                var documentKey = (document.SourceType, document.Id);
                controlLinesByDocument.TryGetValue(documentKey, out var original);
                original ??= [];

                if (original.Length != 1)
                {
                    AddIssue(original.Length == 0
                            ? "MissingOriginalControlLine"
                            : "MultipleOriginalControlLines",
                        kind, document.Id, null,
                        "Expected exactly one historical control-account line on the source journal.");
                }

                var originalLine = original.Length == 1 ? original[0] : null;
                if (originalLine is not null)
                {
                    var expectedStatus = document.Status == "Cancelled"
                        ? "Reversed"
                        : "Posted";
                    if (originalLine.JournalStatus != expectedStatus)
                    {
                        AddIssue("JournalLifecycleMismatch", kind, document.Id,
                            null, "Original GL status differs from the document lifecycle.");
                    }
                }

                var sourceAccount = originalLine?.AccountId;
                var knownAccounts = receivable ? arAccounts : apAccounts;
                var controlId = sourceAccount ?? (
                    knownAccounts.Count == 1 ? knownAccounts.Single() : (Guid?)null);
                if (!controlId.HasValue)
                {
                    AddIssue("UnmappedControlAccount", kind, document.Id, null,
                        "Cannot attribute this open item to a unique GL control account.");
                }

                if (document.Status == "Cancelled")
                {
                    if (allocatedByDocument.TryGetValue(documentKey, out var cancelledAmount)
                        && cancelledAmount > 0m)
                        AddIssue("CancelledDocumentAllocated", kind, document.Id,
                            null, "Cancelled document retains confirmed payment allocations.");
                    continue;
                }

                if (!document.ExchangeRate.HasValue ||
                    document.ExchangeRate.Value <= 0m)
                {
                    AddIssue("MissingDocumentRate", kind, document.Id, null,
                        "Issued/received document has no positive original FX rate.");
                    continue;
                }

                allocatedByDocument.TryGetValue(documentKey, out var settledAmount);
                if (settledAmount > document.GrossAmount)
                {
                    AddIssue("OverallocatedDocument", kind, document.Id, null,
                        "Confirmed allocations exceed the original gross document amount.");
                }

                var outstandingDocument = document.GrossAmount - settledAmount;
                var rate = document.ExchangeRate.Value;
                // Exactly match AutoPostingEngine: each allocated document amount
                // is converted independently to base currency, then aggregated.
                var settledBase = allocations.Where(allocation =>
                        allocation.PaymentStatus == "Confirmed" &&
                        (receivable
                            ? allocation.SalesInvoiceId == document.Id
                            : allocation.PurchaseDocumentId == document.Id))
                    .Sum(allocation => ToBase(allocation.DocumentAmount, rate));
                var outstandingBase = ToBase(document.GrossAmount, rate) - settledBase;

                if (controlId.HasValue)
                {
                    var controlKey = (kind, controlId.Value);
                    expectedByControl.TryGetValue(controlKey, out var existing);
                    expectedByControl[controlKey] = existing + outstandingBase;
                }

                if (outstandingDocument != 0m || outstandingBase != 0m)
                {
                    openItems.Add(new FinancialOpenItemResponse(
                        document.Id, document.SourceType, document.CounterpartyId,
                        document.Number, document.DocumentDate, document.DueDate,
                        document.GrossAmount, settledAmount,
                        outstandingDocument, outstandingBase,
                        controlId, document.DueDate < cutoff &&
                            outstandingDocument > 0m,
                        outstandingDocument > 0m && document.DueDate < cutoff
                            ? cutoff.DayNumber - document.DueDate.DayNumber : 0,
                        AgingBucket(
                            outstandingDocument > 0m && document.DueDate < cutoff
                                ? cutoff.DayNumber - document.DueDate.DayNumber : 0)));
                }
            }

            // Capture GL accounts from both configuration and historical
            // documents. Accounts used before rule changes stay observable.
            foreach (var line in originalLines)
            {
                if (line.SourceType == "SalesInvoice")
                    arAccounts.Add(line.AccountId);
                else
                    apAccounts.Add(line.AccountId);
            }

            foreach (var shared in arAccounts.Intersect(apAccounts))
            {
                AddIssue("SharedControlAccount", "Receivables", null, null,
                    $"Account {shared} is mapped to both AR and AP.");
                AddIssue("SharedControlAccount", "Payables", null, null,
                    $"Account {shared} is mapped to both AR and AP.");
            }

            var ids = arAccounts.Concat(apAccounts).Distinct().ToArray();
            var gl = (await repository.GetControlLedgerBalancesAsync(
                orgId, cutoff, ids, ct))
                .ToDictionary(item => item.AccountId);

            var reconciledControls = new List<FinancialControlAccountReconciliationResponse>();
            foreach (var (kind, accounts) in new[]
                     {
                         (Type: "Receivables", Accounts: arAccounts),
                         (Type: "Payables", Accounts: apAccounts)
                     })
            {
                foreach (var accountId in accounts.OrderBy(id => id))
                {
                    gl.TryGetValue(accountId, out var movements);
                    var actual = movements is null ? 0m :
                        kind == "Receivables"
                            ? movements.Debit - movements.Credit
                            : movements.Credit - movements.Debit;
                    expectedByControl.TryGetValue((kind, accountId), out var expected);
                    var difference = actual - expected;
                    reconciledControls.Add(
                        new FinancialControlAccountReconciliationResponse(
                            accountId, kind, expected, actual, difference,
                            difference == 0m));
                    if (difference != 0m)
                    {
                        AddIssue("ControlAccountDifference", kind, null, null,
                            $"Control {accountId}: expected {expected:F4}, GL {actual:F4}, difference {difference:F4}.");
                    }
                }
            }

            var receivablesOk = !issues.Any(issue =>
                issue.SourceType is "Receivables" or "SalesInvoice" ||
                issue.SourceType == "Payment" &&
                    (issue.Code is "InvalidAllocationTarget" or "MissingAllocatedDocument"));
            var payablesOk = !issues.Any(issue =>
                issue.SourceType is "Payables" or "PurchaseDocument" ||
                issue.SourceType == "Payment" &&
                    (issue.Code is "InvalidAllocationTarget" or "MissingAllocatedDocument"));

            return new FinancialOpenItemReconciliationResponse(
                fiscalYearId, cutoff,
                HistoricalAsOfSnapshotAvailable: false,
                AccountsReceivableReconciled: receivablesOk,
                AccountsPayableReconciled: payablesOk,
                BankStatementsReconciled: false,
                FinancialSubsystemsFullyReconciled: false,
                ReceivablesOpenBase: openItems
                    .Where(item => item.SourceType == "SalesInvoice")
                    .Sum(item => item.OutstandingBaseAmount),
                PayablesOpenBase: openItems
                    .Where(item => item.SourceType == "PurchaseDocument")
                    .Sum(item => item.OutstandingBaseAmount),
                OpenItems: openItems.OrderBy(item => item.CounterpartyId)
                    .ThenBy(item => item.DueDate).ThenBy(item => item.DocumentId)
                    .ToArray(),
                ControlAccounts: reconciledControls.OrderBy(item => item.Type)
                    .ThenBy(item => item.AccountId).ToArray(),
                Issues: issues.OrderBy(issue => issue.SourceType)
                    .ThenBy(issue => issue.DocumentId)
                    .ThenBy(issue => issue.Code).ToArray());
        }, cancellationToken);
    }

    private static string AgingBucket(int daysOverdue) =>
        daysOverdue switch
        {
            <= 0 => "Current",
            <= 30 => "1-30",
            <= 60 => "31-60",
            <= 90 => "61-90",
            _ => "90+"
        };

    private static decimal ToBase(decimal amount, decimal rate) =>
        amount == 0m ? 0m : decimal.Round(
            amount / rate, 4, MidpointRounding.AwayFromZero);
}
