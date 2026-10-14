using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

/// <summary>
/// Reconciles CURRENT sales/purchases/payments document states against
/// original automatically posted GL journals for the selected fiscal year.
/// This is a source-to-ledger integrity check, not aged AR/AP or bank statement
/// reconciliation and not a historical as-of snapshot.
/// </summary>
public sealed class FinancialSourceReconciliationService(
    IFinancialSourceReconciliationRepository repository,
    IFiscalCalendarRepository calendar,
    ITransactionRunner transactions,
    ICurrentOrganization currentOrganization)
    : IFinancialSourceReconciliationService
{
    public async Task<FinancialSourceReconciliationResponse?> ReconcileAsync(
        Guid fiscalYearId, CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        return await transactions.ExecuteAsync(async ct =>
        {
            // GL posting shares this fiscal-year row; holding FOR UPDATE
            // prevents an in-flight posting from changing the ledger
            // while we aggregate its journal evidence.
            await calendar.LockFiscalYearForCloseAsync(
                organizationId, fiscalYearId, ct);
            var year = await calendar.GetFiscalYearAsync(
                organizationId, fiscalYearId, trackChanges: false, ct);
            if (year is null) return null;

            var sources = await repository.ListFinancialSourcesAsync(
                organizationId, year.StartDate, year.EndDate, ct);
            var journals = await repository.ListSourceJournalsAsync(
                organizationId, year.StartDate, year.EndDate, ct);
            var reversedJournalIds = journals
                .Where(journal => journal.ReversedByJournalEntryId.HasValue)
                .Select(journal => journal.Id)
                .Distinct().ToArray();
            var postedReversals = await repository.GetPostedReversalsAsync(
                organizationId, reversedJournalIds, ct);
            var reversalsByOriginal = postedReversals
                .GroupBy(reversal => reversal.ReversalOfJournalEntryId)
                .ToDictionary(group => group.Key, group => group.ToArray());
            var allocations = await repository.ListPaymentAllocationsAsync(
                organizationId, year.StartDate, year.EndDate, ct);
            var allocationsByPayment = allocations.GroupBy(row => row.PaymentId)
                .ToDictionary(group => group.Key, group => group.ToArray());


            var issues = new List<FinancialSourceReconciliationIssue>();
            var journalGroups = journals.GroupBy(journal => journal.ExternalReference)
                .ToDictionary(group => group.Key, group => group.ToArray(),
                    StringComparer.Ordinal);
            var sourceKeys = sources.Select(source => Reference(source))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var source in sources)
            {
                void AddIssue(string code, string detail, Guid? journalId = null) =>
                    issues.Add(new FinancialSourceReconciliationIssue(
                        source.SourceType, source.Id, journalId, code, detail));

                var key = Reference(source);
                journalGroups.TryGetValue(key, out var matching);
                matching ??= [];
                var isDraft = source.Status == "Draft";
                var isCancelled = source.Status == "Cancelled";
                var active = source.Status is "Issued" or "Received" or "Confirmed";

                if (source.SourceType == "Payment" &&
                    source.Status == "Confirmed" &&
                    source.AllocatedAmount is decimal recordedAmount)
                {
                    FinancialPaymentAllocationSnapshot[] rows = allocationsByPayment.TryGetValue(
                        source.Id, out var linked) ? linked : [];
                    var allocatedTotal = rows.Sum(row => row.PaymentAmount);
                    if (allocatedTotal != recordedAmount ||
                        allocatedTotal < 0m ||
                        allocatedTotal > source.GrossAmount)
                    {
                        AddIssue("PaymentAllocationMismatch",
                            "The declared allocated amount differs from the sum of payment-currency allocation rows or exceeds the payment amount.");
                    }
                    if (rows.Any(row => row.PaymentAmount <= 0m ||
                        row.DocumentAmount <= 0m ||
                        (source.Direction == "Incoming"
                            ? row.SalesInvoiceId is null || row.PurchaseDocumentId is not null
                            : row.PurchaseDocumentId is null || row.SalesInvoiceId is not null)))
                    {
                        AddIssue("InvalidAllocationTarget",
                            "Allocation amount or target type does not match payment direction.");
                    }
                }

                if (isDraft)
                {
                    if (matching.Length > 0)
                    {
                        AddIssue("UnexpectedPosting",
                            "Draft source document already has a ledger journal.");
                    }
                    continue;
                }

                if (!active && !isCancelled)
                {
                    AddIssue("UnsupportedStatus",
                        "Unknown financial source status cannot be reconciled.");
                    continue;
                }

                if (matching.Length == 0)
                {
                    AddIssue("MissingPosting",
                        "Finalized financial source has no original system journal.");
                    continue;
                }

                if (matching.Length != 1)
                {
                    AddIssue("DuplicatePosting",
                        "Financial source has multiple original system journals.");
                    continue;
                }

                var journal = matching[0];
                if (journal.PostingDate != source.DocumentDate)
                {
                    AddIssue("PostingDateMismatch",
                        "Original journal posting date differs from financial source date.");
                }

                if (journal.Status != (isCancelled ? "Reversed" : "Posted"))
                {
                    AddIssue("PostingStatusMismatch",
                        "Source lifecycle does not match the original journal status.");
                }

                if (isCancelled)
                {
                    if (journal.ReversedByJournalEntryId is not Guid reversalId ||
                        !reversalsByOriginal.TryGetValue(journal.Id, out var reversals) ||
                        reversals.Length != 1 || reversals[0].Id != reversalId)
                    {
                        AddIssue("MissingReversal",
                            "Cancelled financial source requires a linked posted reversing journal.",
                            journal.Id);
                    }
                    else if (!IsExactReversal(journal.Lines, reversals[0].Lines))
                    {
                        AddIssue("ReversalLineMismatch",
                            "The posted reversal does not offset every original GL account and journal line.",
                            journal.Id);
                    }
                }

                if (journal.Lines.Count < 2 ||
                    journal.Lines.Sum(line => line.Debit) <= 0m ||
                    journal.Lines.Sum(line => line.Debit) !=
                        journal.Lines.Sum(line => line.Credit))
                {
                    AddIssue("UnbalancedPosting",
                        "Source journal is missing lines or has unequal debit/credit totals.");
                }

                if (source.ExchangeRate is not decimal rate || rate <= 0m)
                {
                    AddIssue("MissingExchangeRate",
                        "Finalized source is missing a valid original FX rate.");
                    continue;
                }

                // Same conversion policy as AutoPostingEngine.ToBase.
                var expected = decimal.Round(source.GrossAmount / rate,
                    4, MidpointRounding.AwayFromZero);
                var control = journal.Lines.Where(line =>
                    source.SourceType switch
                    {
                        "SalesInvoice" => line.Description?.StartsWith(
                            "Accounts receivable -", StringComparison.Ordinal) == true,
                        "PurchaseDocument" => line.Description?.StartsWith(
                            "Accounts payable -", StringComparison.Ordinal) == true,
                        "Payment" => source.Direction is "Incoming" or "Outgoing" &&
                            line.Description?.StartsWith(source.Direction + " ",
                                StringComparison.Ordinal) == true,
                        _ => false
                    }).ToArray();

                var debitExpected = source.SourceType == "SalesInvoice" ||
                    (source.SourceType == "Payment" &&
                     source.Direction == "Incoming");
                if (control.Length != 1 ||
                    (debitExpected
                        ? control[0].Debit != expected || control[0].Credit != 0m
                        : control[0].Credit != expected || control[0].Debit != 0m))
                {
                    AddIssue("ControlAmountMismatch",
                        "Document base-currency gross amount does not match its automatically posted control line.");
                }

            }

            foreach (var journal in journals)
            {
                if (!sourceKeys.Contains(journal.ExternalReference))
                {
                    issues.Add(new FinancialSourceReconciliationIssue(
                        SourceTypeFromReference(journal.ExternalReference),
                        null, journal.Id, "OrphanPosting",
                        "System financial journal has no source document dated within this fiscal year."));
                }
            }

            static bool Clean(string kind,
                IReadOnlyList<FinancialSourceReconciliationIssue> problems) =>
                !problems.Any(issue => issue.SourceType == kind);

            var salesValid = Clean("SalesInvoice", issues);
            var purchasesValid = Clean("PurchaseDocument", issues);
            var paymentsValid = Clean("Payment", issues);

            return new FinancialSourceReconciliationResponse(
                fiscalYearId, year.StartDate, year.EndDate,
                sources.Count, journals.Count, issues.Count,
                salesValid, purchasesValid, paymentsValid,
                BankingReconciliationAvailable: false,
                FinancialSubsystemsFullyReconciled: false,
                issues.OrderBy(issue => issue.SourceType)
                    .ThenBy(issue => issue.SourceId)
                    .ThenBy(issue => issue.Code).ToArray());
        }, cancellationToken);
    }

    private static bool IsExactReversal(
        IReadOnlyList<FinancialSourceJournalLineSnapshot> original,
        IReadOnlyList<FinancialSourceJournalLineSnapshot> compensation)
    {
        if (original.Count == 0 || original.Count != compensation.Count)
            return false;

        var a = original.OrderBy(line => line.LineNumber).ToArray();
        var b = compensation.OrderBy(line => line.LineNumber).ToArray();
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i].LineNumber != b[i].LineNumber ||
                a[i].AccountId != b[i].AccountId ||
                a[i].Debit != b[i].Credit ||
                a[i].Credit != b[i].Debit)
                return false;
        }
        return true;
    }

    private static string Reference(FinancialSourcePostingSnapshot source)
    {
        var prefix = source.SourceType switch
        {
            "SalesInvoice" => "sales-invoice",
            "PurchaseDocument" => "purchase-document",
            "Payment" => "payment",
            _ => throw new InvalidOperationException(
                "Unsupported financial source reconciliation type.")
        };
        return $"{prefix}:{source.Id:N}";
    }

    private static string SourceTypeFromReference(string reference)
    {
        if (reference.StartsWith("sales-invoice:", StringComparison.Ordinal))
            return "SalesInvoice";
        if (reference.StartsWith("purchase-document:", StringComparison.Ordinal))
            return "PurchaseDocument";
        return "Payment";
    }
}
