using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Payments;
using Fintrox.Domain.Purchases;
using Fintrox.Domain.Sales;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

public sealed class FinancialSourceReconciliationRepository(
    FintroxDbContext dbContext) : IFinancialSourceReconciliationRepository
{
    public async Task<IReadOnlyList<FinancialSourcePostingSnapshot>> ListFinancialSourcesAsync(
        Guid organizationId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var sales = await dbContext.SalesInvoices.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.InvoiceDate >= from && x.InvoiceDate <= to)
            .Select(x => new
            {
                x.Id, x.Status, Date = x.InvoiceDate, Amount = x.GrossTotal,
                x.ExchangeRate
            })
            .ToArrayAsync(cancellationToken);
        var purchases = await dbContext.PurchaseDocuments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.DocumentDate >= from && x.DocumentDate <= to)
            .Select(x => new
            {
                x.Id, x.Status, Date = x.DocumentDate, Amount = x.GrossTotal,
                x.ExchangeRate
            })
            .ToArrayAsync(cancellationToken);
        var payments = await dbContext.Payments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.PaymentDate >= from && x.PaymentDate <= to)
            .Select(x => new
            {
                x.Id, x.Status, Date = x.PaymentDate, Amount = x.Amount,
                x.ExchangeRate, x.Direction, x.AllocatedAmount
            })
            .ToArrayAsync(cancellationToken);

        var result = new List<FinancialSourcePostingSnapshot>(
            sales.Length + purchases.Length + payments.Length);
        result.AddRange(sales.Select(x => new FinancialSourcePostingSnapshot(
            x.Id, "SalesInvoice", x.Status.ToString(), x.Date,
            x.Amount, x.ExchangeRate, null, null)));
        result.AddRange(purchases.Select(x => new FinancialSourcePostingSnapshot(
            x.Id, "PurchaseDocument", x.Status.ToString(), x.Date,
            x.Amount, x.ExchangeRate, null, null)));
        result.AddRange(payments.Select(x => new FinancialSourcePostingSnapshot(
            x.Id, "Payment", x.Status.ToString(), x.Date,
            x.Amount, x.ExchangeRate, x.Direction.ToString(), x.AllocatedAmount)));
        return result;
    }

    public async Task<IReadOnlyList<FinancialSourceJournalSnapshot>> ListSourceJournalsAsync(
        Guid organizationId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        // Only ORIGINAL automatically generated source journals. Compensating
        // reversals have no external reference and are checked separately.
        var entries = await dbContext.JournalEntries.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.Source == JournalEntrySource.System &&
                x.PostingDate >= from && x.PostingDate <= to &&
                x.ExternalReference != null &&
                (x.ExternalReference.StartsWith("sales-invoice:") ||
                 x.ExternalReference.StartsWith("purchase-document:") ||
                 x.ExternalReference.StartsWith("payment:")))
            .Select(x => new
            {
                x.Id, x.ExternalReference, x.Status,
                x.PostingDate, x.ReversedByJournalEntryId
            })
            .ToArrayAsync(cancellationToken);

        var ids = entries.Select(x => x.Id).ToArray();
        var lines = await dbContext.JournalLines.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                ids.Contains(x.JournalEntryId))
            .Select(x => new
            {
                x.JournalEntryId, x.LineNumber, x.AccountId, x.Debit, x.Credit,
                x.Description
            })
            .ToArrayAsync(cancellationToken);
        var byEntry = lines.GroupBy(x => x.JournalEntryId)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.LineNumber)
                .Select(y => new FinancialSourceJournalLineSnapshot(
                    y.LineNumber, y.AccountId,
                    y.Debit, y.Credit, y.Description)).ToArray());

        return entries.Select(x => new FinancialSourceJournalSnapshot(
            x.Id, x.ExternalReference!,
            x.Status.ToString(), x.PostingDate,
            x.ReversedByJournalEntryId,
            byEntry.TryGetValue(x.Id, out var journalLines)
                ? journalLines
                : [])).ToArray();
    }

    public async Task<IReadOnlyList<FinancialPaymentAllocationSnapshot>>
        ListPaymentAllocationsAsync(
            Guid organizationId, DateOnly from, DateOnly to,
            CancellationToken cancellationToken)
    {
        return await (
            from allocation in dbContext.PaymentAllocations.AsNoTracking()
            join payment in dbContext.Payments.AsNoTracking()
                on new { allocation.PaymentId, allocation.OrganizationId }
                equals new { PaymentId = payment.Id, payment.OrganizationId }
            where allocation.OrganizationId == organizationId &&
                payment.PaymentDate >= from && payment.PaymentDate <= to
            select new FinancialPaymentAllocationSnapshot(
                allocation.PaymentId, allocation.PaymentAmount,
                allocation.DocumentAmount,
                allocation.SalesInvoiceId, allocation.PurchaseDocumentId)
        ).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialPostedReversalSnapshot>>
        GetPostedReversalsAsync(
            Guid organizationId, IReadOnlyList<Guid> originalIds,
            CancellationToken cancellationToken)
    {
        if (originalIds.Count == 0) return [];

        var entries = await dbContext.JournalEntries.AsNoTracking()
            .Where(entry => entry.OrganizationId == organizationId &&
                entry.Status == JournalEntryStatus.Posted &&
                entry.ReversalOfJournalEntryId != null &&
                originalIds.Contains(entry.ReversalOfJournalEntryId.Value))
            .Select(entry => new
            {
                entry.Id, entry.ReversalOfJournalEntryId
            })
            .ToArrayAsync(cancellationToken);
        var journalIds = entries.Select(entry => entry.Id).ToArray();
        var lines = await dbContext.JournalLines.AsNoTracking()
            .Where(line => line.OrganizationId == organizationId &&
                journalIds.Contains(line.JournalEntryId))
            .Select(line => new
            {
                line.JournalEntryId, line.LineNumber, line.AccountId,
                line.Debit, line.Credit, line.Description
            })
            .ToArrayAsync(cancellationToken);

        var byEntry = lines.GroupBy(line => line.JournalEntryId)
            .ToDictionary(group => group.Key, group => group
                .OrderBy(line => line.LineNumber)
                .Select(line => new FinancialSourceJournalLineSnapshot(
                    line.LineNumber, line.AccountId,
                    line.Debit, line.Credit, line.Description))
                .ToArray());

        return entries.Select(entry => new FinancialPostedReversalSnapshot(
            entry.Id, entry.ReversalOfJournalEntryId!.Value,
            byEntry.TryGetValue(entry.Id, out var reversalLines)
                ? reversalLines : [])).ToArray();
    }
}
