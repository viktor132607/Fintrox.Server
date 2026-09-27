using System.Globalization;
using Fintrox.Application.Purchases;
using Fintrox.Application.Sales;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;

namespace Fintrox.Application.Accounting;

public sealed class AutoPostingEngine(
    IAutoPostingRuleRepository ruleRepository,
    IJournalRepository journalRepository,
    IAccountRepository accountRepository,
    IFiscalCalendarRepository fiscalCalendarRepository,
    ISalesInvoiceRepository salesInvoiceRepository,
    IPurchaseDocumentRepository purchaseDocumentRepository,
    TimeProvider timeProvider) : IAutoPostingEngine
{
    public async Task PostSalesInvoiceAsync(
        SalesInvoice invoice,
        IReadOnlyList<SalesInvoiceLine> lines,
        CancellationToken cancellationToken)
    {
        var rate = RequireRate(invoice.ExchangeRate, "sales invoice");
        var rules = await LoadRulesAsync(
            invoice.OrganizationId,
            cancellationToken);

        var postings = new List<PendingPosting>();
        var receivable = ResolveRule(
            rules,
            PostingComponent.AccountsReceivable);

        var revenueGroups = lines
            .GroupBy(line => ResolveRule(
                rules,
                PostingComponent.Revenue,
                PostingRuleMatchKind.ItemCode,
                line.ItemCode).AccountId)
            .Select(group => new PendingPosting(
                group.Key,
                Debit: 0m,
                Credit: ToBase(group.Sum(line => line.NetAmount), rate),
                "Sales revenue"))
            .ToList();

        var vatGroups = lines
            .Where(line => line.VatAmount > 0m)
            .GroupBy(line => ResolveRule(
                rules,
                PostingComponent.OutputVat,
                PostingRuleMatchKind.VatCode,
                line.VatCode).AccountId)
            .Select(group => new PendingPosting(
                group.Key,
                Debit: 0m,
                Credit: ToBase(group.Sum(line => line.VatAmount), rate),
                "Output VAT"))
            .ToList();

        var grossBase = ToBase(invoice.GrossTotal, rate);
        postings.Add(new PendingPosting(
            receivable.AccountId,
            grossBase,
            0m,
            $"Accounts receivable - {invoice.CustomerName}"));
        postings.AddRange(revenueGroups);
        postings.AddRange(vatGroups);

        BalanceCreditsToDebit(postings, grossBase);

        await PostEntryAsync(
            invoice.OrganizationId,
            invoice.InvoiceDate,
            invoice.InvoiceDate,
            $"Sales invoice {invoice.Number ?? invoice.Id.ToString()}",
            SalesReference(invoice.Id),
            postings,
            cancellationToken);
    }

    public Task ReverseSalesInvoiceAsync(
        SalesInvoice invoice,
        string reason,
        CancellationToken cancellationToken) =>
        ReverseEntryAsync(
            invoice.OrganizationId,
            SalesReference(invoice.Id),
            invoice.InvoiceDate,
            reason,
            cancellationToken);

    public async Task PostPurchaseDocumentAsync(
        PurchaseDocument document,
        IReadOnlyList<PurchaseDocumentLine> lines,
        CancellationToken cancellationToken)
    {
        var rate = RequireRate(document.ExchangeRate, "purchase document");
        var rules = await LoadRulesAsync(
            document.OrganizationId,
            cancellationToken);

        var postings = new List<PendingPosting>();
        var payable = ResolveRule(
            rules,
            PostingComponent.AccountsPayable);

        var expenseGroups = lines
            .GroupBy(line => ResolveRule(
                rules,
                PostingComponent.Expense,
                PostingRuleMatchKind.ItemCode,
                line.ItemCode).AccountId)
            .Select(group => new PendingPosting(
                group.Key,
                ToBase(
                    group.Sum(line =>
                        line.NetAmount + line.NonRecoverableVatAmount),
                    rate),
                Credit: 0m,
                "Purchase expense"))
            .ToList();

        var vatGroups = lines
            .Where(line => line.RecoverableVatAmount > 0m)
            .GroupBy(line => ResolveRule(
                rules,
                PostingComponent.InputVat,
                PostingRuleMatchKind.VatCode,
                line.VatCode).AccountId)
            .Select(group => new PendingPosting(
                group.Key,
                ToBase(
                    group.Sum(line => line.RecoverableVatAmount),
                    rate),
                Credit: 0m,
                "Recoverable input VAT"))
            .ToList();

        var grossBase = ToBase(document.GrossTotal, rate);
        postings.AddRange(expenseGroups);
        postings.AddRange(vatGroups);
        postings.Add(new PendingPosting(
            payable.AccountId,
            Debit: 0m,
            Credit: grossBase,
            $"Accounts payable - {document.SupplierName}"));

        BalanceDebitsToCredit(postings, grossBase);

        await PostEntryAsync(
            document.OrganizationId,
            document.DocumentDate,
            document.DocumentDate,
            $"Purchase document {document.InternalNumber ?? document.Id.ToString()}",
            PurchaseReference(document.Id),
            postings,
            cancellationToken);
    }

    public Task ReversePurchaseDocumentAsync(
        PurchaseDocument document,
        string reason,
        CancellationToken cancellationToken) =>
        ReverseEntryAsync(
            document.OrganizationId,
            PurchaseReference(document.Id),
            document.DocumentDate,
            reason,
            cancellationToken);

    public async Task PostPaymentAsync(
        Payment payment,
        IReadOnlyList<PaymentAllocation> allocations,
        CancellationToken cancellationToken)
    {
        var paymentRate = RequireRate(payment.ExchangeRate, "payment");
        var rules = await LoadRulesAsync(
            payment.OrganizationId,
            cancellationToken);

        var asset = ResolveRule(
            rules,
            PostingComponent.PaymentAsset,
            PostingRuleMatchKind.PaymentMethod,
            payment.Method.ToString());

        var paymentBase = ToBase(payment.Amount, paymentRate);
        var allocatedCarryingBase = 0m;
        var postings = new List<PendingPosting>();

        if (payment.Direction == PaymentDirection.Incoming)
        {
            var receivable = ResolveRule(
                rules,
                PostingComponent.AccountsReceivable);

            foreach (var allocation in allocations)
            {
                if (!allocation.SalesInvoiceId.HasValue)
                {
                    throw new AutoPostingException(
                        "Incoming payment allocation does not reference a sales invoice.");
                }

                var invoice = await salesInvoiceRepository.GetInvoiceAsync(
                    payment.OrganizationId,
                    allocation.SalesInvoiceId.Value,
                    trackChanges: false,
                    cancellationToken);

                if (invoice is null)
                {
                    throw new AutoPostingException(
                        "An allocated sales invoice no longer exists.");
                }

                allocatedCarryingBase += ToBase(
                    allocation.DocumentAmount,
                    RequireRate(invoice.ExchangeRate, "allocated sales invoice"));
            }

            if (allocatedCarryingBase > 0m)
            {
                postings.Add(new PendingPosting(
                    receivable.AccountId,
                    Debit: 0m,
                    Credit: allocatedCarryingBase,
                    "Settle accounts receivable"));
            }

            var unallocatedBase = ToBase(
                payment.UnallocatedAmount,
                paymentRate);

            if (unallocatedBase > 0m)
            {
                var advance = ResolveRule(
                    rules,
                    PostingComponent.CustomerAdvance);

                postings.Add(new PendingPosting(
                    advance.AccountId,
                    Debit: 0m,
                    Credit: unallocatedBase,
                    "Customer advance / unapplied receipt"));
            }

            var creditedBase = allocatedCarryingBase + unallocatedBase;
            var difference = paymentBase - creditedBase;

            if (difference > 0m)
            {
                var gain = ResolveRule(rules, PostingComponent.FxGain);
                postings.Add(new PendingPosting(
                    gain.AccountId,
                    Debit: 0m,
                    Credit: difference,
                    "Realized FX gain"));
            }
            else if (difference < 0m)
            {
                var loss = ResolveRule(rules, PostingComponent.FxLoss);
                postings.Add(new PendingPosting(
                    loss.AccountId,
                    Debit: -difference,
                    Credit: 0m,
                    "Realized FX loss"));
            }

            postings.Insert(0, new PendingPosting(
                asset.AccountId,
                Debit: paymentBase,
                Credit: 0m,
                $"Incoming {payment.Method}"));
        }
        else
        {
            var payable = ResolveRule(
                rules,
                PostingComponent.AccountsPayable);

            foreach (var allocation in allocations)
            {
                if (!allocation.PurchaseDocumentId.HasValue)
                {
                    throw new AutoPostingException(
                        "Outgoing payment allocation does not reference a purchase document.");
                }

                var document = await purchaseDocumentRepository.GetAsync(
                    payment.OrganizationId,
                    allocation.PurchaseDocumentId.Value,
                    trackChanges: false,
                    cancellationToken);

                if (document is null)
                {
                    throw new AutoPostingException(
                        "An allocated purchase document no longer exists.");
                }

                allocatedCarryingBase += ToBase(
                    allocation.DocumentAmount,
                    RequireRate(document.ExchangeRate, "allocated purchase document"));
            }

            if (allocatedCarryingBase > 0m)
            {
                postings.Add(new PendingPosting(
                    payable.AccountId,
                    Debit: allocatedCarryingBase,
                    Credit: 0m,
                    "Settle accounts payable"));
            }

            var unallocatedBase = ToBase(
                payment.UnallocatedAmount,
                paymentRate);

            if (unallocatedBase > 0m)
            {
                var advance = ResolveRule(
                    rules,
                    PostingComponent.SupplierAdvance);

                postings.Add(new PendingPosting(
                    advance.AccountId,
                    Debit: unallocatedBase,
                    Credit: 0m,
                    "Supplier advance / unapplied payment"));
            }

            var debitedBase = allocatedCarryingBase + unallocatedBase;
            var difference = paymentBase - debitedBase;

            if (difference > 0m)
            {
                var loss = ResolveRule(rules, PostingComponent.FxLoss);
                postings.Add(new PendingPosting(
                    loss.AccountId,
                    Debit: difference,
                    Credit: 0m,
                    "Realized FX loss"));
            }
            else if (difference < 0m)
            {
                var gain = ResolveRule(rules, PostingComponent.FxGain);
                postings.Add(new PendingPosting(
                    gain.AccountId,
                    Debit: 0m,
                    Credit: -difference,
                    "Realized FX gain"));
            }

            postings.Add(new PendingPosting(
                asset.AccountId,
                Debit: 0m,
                Credit: paymentBase,
                $"Outgoing {payment.Method}"));
        }

        await PostEntryAsync(
            payment.OrganizationId,
            payment.PaymentDate,
            payment.PaymentDate,
            $"{payment.Direction} payment {payment.InternalNumber ?? payment.Id.ToString()}",
            PaymentReference(payment.Id),
            postings,
            cancellationToken);
    }

    public Task ReversePaymentAsync(
        Payment payment,
        string reason,
        CancellationToken cancellationToken) =>
        ReverseEntryAsync(
            payment.OrganizationId,
            PaymentReference(payment.Id),
            payment.PaymentDate,
            reason,
            cancellationToken);

    private async Task<IReadOnlyList<AutoPostingRule>> LoadRulesAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.ListActiveAsync(
            organizationId,
            cancellationToken);

        if (rules.Count == 0)
        {
            throw new AutoPostingException(
                "No active accounting auto-posting rules are configured for this organization.");
        }

        return rules;
    }

    private static AutoPostingRule ResolveRule(
        IReadOnlyList<AutoPostingRule> rules,
        PostingComponent component,
        PostingRuleMatchKind? matchKind = null,
        string? matchValue = null)
    {
        if (matchKind.HasValue &&
            !string.IsNullOrWhiteSpace(matchValue))
        {
            var normalized = AutoPostingRule.NormalizeMatchValue(
                matchKind.Value,
                matchValue);

            var exact = rules.SingleOrDefault(rule =>
                rule.Component == component &&
                rule.MatchKind == matchKind.Value &&
                rule.MatchValue == normalized);

            if (exact is not null)
            {
                return exact;
            }
        }

        var fallback = rules.SingleOrDefault(rule =>
            rule.Component == component &&
            rule.MatchKind == PostingRuleMatchKind.Default &&
            rule.MatchValue == "*");

        return fallback ?? throw new AutoPostingException(
            $"No active auto-posting rule resolves component '{component}' for value '{matchValue ?? "*"}'.");
    }

    private async Task PostEntryAsync(
        Guid organizationId,
        DateOnly postingDate,
        DateOnly documentDate,
        string description,
        string externalReference,
        IReadOnlyList<PendingPosting> postings,
        CancellationToken cancellationToken)
    {
        var existing = await journalRepository.GetSystemEntryByExternalReferenceAsync(
            organizationId,
            externalReference,
            trackChanges: false,
            cancellationToken);

        if (existing is not null)
        {
            throw new AutoPostingException(
                $"A system journal entry already exists for '{externalReference}'.");
        }

        var (period, fiscalYear) = await ResolveOpenPostingContextAsync(
            organizationId,
            postingDate,
            cancellationToken);

        var normalized = postings
            .Where(item => item.Debit > 0m || item.Credit > 0m)
            .ToArray();

        if (normalized.Length < 2)
        {
            throw new AutoPostingException(
                "Auto-posting must produce at least two journal lines.");
        }

        var debitTotal = normalized.Sum(item => item.Debit);
        var creditTotal = normalized.Sum(item => item.Credit);

        if (debitTotal <= 0m || debitTotal != creditTotal)
        {
            throw new AutoPostingException(
                $"Auto-posting is not balanced. Debit={debitTotal:F4}, Credit={creditTotal:F4}.");
        }

        var accountIds = normalized
            .Select(item => item.AccountId)
            .Distinct()
            .ToArray();

        var accounts = await accountRepository.ListByIdsAsync(
            organizationId,
            accountIds,
            cancellationToken);

        if (accounts.Count != accountIds.Length)
        {
            throw new AutoPostingException(
                "An auto-posting rule references a missing account.");
        }

        foreach (var account in accounts.Values)
        {
            if (!account.IsActive)
            {
                throw new AutoPostingException(
                    $"Account '{account.Code}' is inactive.");
            }
        }

        var now = timeProvider.GetUtcNow();
        var entry = JournalEntry.CreateDraft(
            organizationId,
            period.Id,
            postingDate,
            documentDate,
            description,
            JournalEntrySource.System,
            externalReference,
            now);

        await journalRepository.AddEntryAsync(entry, cancellationToken);

        var lineNumber = 1;
        foreach (var posting in normalized)
        {
            await journalRepository.AddLineAsync(
                JournalLine.Create(
                    organizationId,
                    entry.Id,
                    lineNumber++,
                    posting.AccountId,
                    posting.Debit,
                    posting.Credit,
                    posting.Description,
                    now),
                cancellationToken);
        }

        await journalRepository.SaveChangesAsync(cancellationToken);

        var sequence = await journalRepository.AllocatePostingSequenceAsync(
            organizationId,
            period.FiscalYearId,
            cancellationToken);

        entry.Post(
            FormatJournalNumber(fiscalYear.StartDate, sequence),
            now);

        await journalRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ReverseEntryAsync(
        Guid organizationId,
        string externalReference,
        DateOnly postingDate,
        string reason,
        CancellationToken cancellationToken)
    {
        var original = await journalRepository.GetSystemEntryByExternalReferenceAsync(
            organizationId,
            externalReference,
            trackChanges: true,
            cancellationToken);

        if (original is null || original.Status == JournalEntryStatus.Reversed)
        {
            return;
        }

        if (original.Status != JournalEntryStatus.Posted)
        {
            throw new AutoPostingException(
                "The source system journal entry is not posted and cannot be reversed.");
        }

        var (period, fiscalYear) = await ResolveOpenPostingContextAsync(
            organizationId,
            postingDate,
            cancellationToken);

        var originalLines = await journalRepository.ListLinesAsync(
            organizationId,
            original.Id,
            cancellationToken);

        if (originalLines.Count < 2)
        {
            throw new AutoPostingException(
                "The source journal entry does not contain enough lines to reverse.");
        }

        var now = timeProvider.GetUtcNow();
        var normalizedReason = string.IsNullOrWhiteSpace(reason)
            ? "source cancellation"
            : reason.Trim();

        var reversal = JournalEntry.CreateReversalDraft(
            organizationId,
            period.Id,
            original.Id,
            postingDate,
            $"Automatic reversal of {original.Number ?? externalReference}: {normalizedReason}",
            now);

        await journalRepository.AddEntryAsync(
            reversal,
            cancellationToken);

        foreach (var line in originalLines)
        {
            await journalRepository.AddLineAsync(
                JournalLine.Create(
                    organizationId,
                    reversal.Id,
                    line.LineNumber,
                    line.AccountId,
                    line.Credit,
                    line.Debit,
                    line.Description,
                    now),
                cancellationToken);
        }

        await journalRepository.SaveChangesAsync(cancellationToken);

        var sequence = await journalRepository.AllocatePostingSequenceAsync(
            organizationId,
            period.FiscalYearId,
            cancellationToken);

        reversal.Post(
            FormatJournalNumber(fiscalYear.StartDate, sequence),
            now);
        original.MarkReversed(reversal.Id, now);

        await journalRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<(AccountingPeriod Period, FiscalYear FiscalYear)>
        ResolveOpenPostingContextAsync(
            Guid organizationId,
            DateOnly postingDate,
            CancellationToken cancellationToken)
    {
        var period = await fiscalCalendarRepository.FindPeriodByDateAsync(
            organizationId,
            postingDate,
            trackChanges: false,
            cancellationToken);

        if (period is null)
        {
            throw new AutoPostingException(
                "No accounting period contains the posting date.");
        }

        if (period.Status != AccountingPeriodStatus.Open)
        {
            throw new AutoPostingException(
                "Automatic posting is allowed only in an open accounting period.");
        }

        var fiscalYear = await fiscalCalendarRepository.GetFiscalYearAsync(
            organizationId,
            period.FiscalYearId,
            trackChanges: false,
            cancellationToken);

        if (fiscalYear is null || fiscalYear.Status != FiscalYearStatus.Open)
        {
            throw new AutoPostingException(
                "Automatic posting is allowed only in an open fiscal year.");
        }

        return (period, fiscalYear);
    }

    private static void BalanceCreditsToDebit(
        List<PendingPosting> postings,
        decimal debitTotal)
    {
        var creditTotal = postings.Sum(item => item.Credit);
        var delta = debitTotal - creditTotal;

        if (delta == 0m)
        {
            return;
        }

        var index = postings.FindIndex(item => item.Credit > 0m);

        if (index < 0 || postings[index].Credit + delta <= 0m)
        {
            throw new AutoPostingException(
                "Unable to balance sales auto-posting after currency conversion.");
        }

        postings[index] = postings[index] with
        {
            Credit = postings[index].Credit + delta
        };
    }

    private static void BalanceDebitsToCredit(
        List<PendingPosting> postings,
        decimal creditTotal)
    {
        var debitTotal = postings.Sum(item => item.Debit);
        var delta = creditTotal - debitTotal;

        if (delta == 0m)
        {
            return;
        }

        var index = postings.FindIndex(item => item.Debit > 0m);

        if (index < 0 || postings[index].Debit + delta <= 0m)
        {
            throw new AutoPostingException(
                "Unable to balance purchase auto-posting after currency conversion.");
        }

        postings[index] = postings[index] with
        {
            Debit = postings[index].Debit + delta
        };
    }

    private static decimal ToBase(decimal amount, decimal rate)
    {
        if (amount == 0m)
        {
            return 0m;
        }

        return decimal.Round(
            amount / rate,
            4,
            MidpointRounding.AwayFromZero);
    }

    private static decimal RequireRate(
        decimal? rate,
        string source)
    {
        if (!rate.HasValue || rate.Value <= 0m)
        {
            throw new AutoPostingException(
                $"The {source} does not contain a valid exchange-rate snapshot.");
        }

        return rate.Value;
    }

    private static string FormatJournalNumber(
        DateOnly fiscalYearStartDate,
        long sequence)
    {
        if (sequence <= 0)
        {
            throw new AutoPostingException(
                "Journal posting sequence must be positive.");
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{fiscalYearStartDate:yyyyMMdd}-{sequence:D6}");
    }

    private static string SalesReference(Guid id) =>
        $"sales-invoice:{id:N}";

    private static string PurchaseReference(Guid id) =>
        $"purchase-document:{id:N}";

    private static string PaymentReference(Guid id) =>
        $"payment:{id:N}";

    private sealed record PendingPosting(
        Guid AccountId,
        decimal Debit,
        decimal Credit,
        string Description);
}
