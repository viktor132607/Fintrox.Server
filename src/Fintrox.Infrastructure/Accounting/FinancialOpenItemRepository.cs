using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

/// <summary>Tenant-safe, no-tracking snapshots of the existing operational ledger.</summary>
public sealed class FinancialOpenItemRepository(
    FintroxDbContext dbContext) : IFinancialOpenItemRepository
{
    public async Task<IReadOnlyList<FinancialOpenDocumentSnapshot>> GetDocumentsAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken)
    {
        var sales = await dbContext.SalesInvoices.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.InvoiceDate <= cutoff)
            .Select(x => new
            {
                x.Id, x.CounterpartyId, x.Number, x.Status,
                Date = x.InvoiceDate, x.DueDate,
                x.GrossTotal, x.ExchangeRate
            })
            .ToArrayAsync(cancellationToken);
        var purchases = await dbContext.PurchaseDocuments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.DocumentDate <= cutoff)
            .Select(x => new
            {
                x.Id, x.CounterpartyId, Number = x.InternalNumber, x.Status,
                Date = x.DocumentDate, x.DueDate,
                x.GrossTotal, x.ExchangeRate
            })
            .ToArrayAsync(cancellationToken);

        return sales.Select(x => new FinancialOpenDocumentSnapshot(
                x.Id, "SalesInvoice", x.CounterpartyId,
                x.Number, x.Status.ToString(), x.Date, x.DueDate,
                x.GrossTotal, x.ExchangeRate))
            .Concat(purchases.Select(x => new FinancialOpenDocumentSnapshot(
                x.Id, "PurchaseDocument", x.CounterpartyId,
                x.Number, x.Status.ToString(), x.Date, x.DueDate,
                x.GrossTotal, x.ExchangeRate)))
            .ToArray();
    }

    public async Task<IReadOnlyList<FinancialOpenAllocationSnapshot>> GetAllocationsAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken)
    {
        var rows = await (
            from allocation in dbContext.PaymentAllocations.AsNoTracking()
            join payment in dbContext.Payments.AsNoTracking()
                on new { allocation.PaymentId, allocation.OrganizationId }
                equals new { PaymentId = payment.Id, payment.OrganizationId }
            where payment.OrganizationId == organizationId &&
                  payment.PaymentDate <= cutoff
            select new
            {
                allocation.PaymentId, payment.CounterpartyId,
                allocation.SalesInvoiceId, allocation.PurchaseDocumentId,
                payment.Status, payment.Direction, payment.PaymentDate,
                allocation.DocumentAmount, allocation.PaymentAmount,
                payment.AllocatedAmount, payment.Amount
            }).ToArrayAsync(cancellationToken);

        return rows.Select(x => new FinancialOpenAllocationSnapshot(
            x.PaymentId, x.CounterpartyId,
            x.SalesInvoiceId, x.PurchaseDocumentId,
            x.Status.ToString(), x.Direction.ToString(),
            x.PaymentDate, x.DocumentAmount, x.PaymentAmount,
            x.AllocatedAmount, x.Amount))
            .ToArray();
    }

    public async Task<IReadOnlyList<FinancialDocumentControlSnapshot>> GetOriginalControlLinesAsync(
        Guid organizationId, DateOnly cutoff, CancellationToken cancellationToken)
    {
        // Same immutable source references and control-line descriptions
        // written by AutoPostingEngine. Parse GUIDs client-side only.
        var rows = await (
            from line in dbContext.JournalLines.AsNoTracking()
            join entry in dbContext.JournalEntries.AsNoTracking()
                on new { line.JournalEntryId, line.OrganizationId }
                equals new { JournalEntryId = entry.Id, entry.OrganizationId }
            where entry.OrganizationId == organizationId &&
                entry.PostingDate <= cutoff &&
                entry.Source == JournalEntrySource.System &&
                entry.ExternalReference != null &&
                (entry.ExternalReference.StartsWith("sales-invoice:") ||
                 entry.ExternalReference.StartsWith("purchase-document:")) &&
                (line.Description!.StartsWith("Accounts receivable -") ||
                 line.Description!.StartsWith("Accounts payable -"))
            select new
            {
                entry.Id, entry.ExternalReference, entry.Status,
                line.AccountId, line.Debit, line.Credit
            }).ToArrayAsync(cancellationToken);

        var result = new List<FinancialDocumentControlSnapshot>(rows.Length);
        foreach (var row in rows)
        {
            var reference = row.ExternalReference!;
            var sales = reference.StartsWith("sales-invoice:", StringComparison.Ordinal);
            var prefix = sales ? "sales-invoice:" : "purchase-document:";
            if (!Guid.TryParseExact(reference[prefix.Length..], "N", out var id))
                continue;

            result.Add(new FinancialDocumentControlSnapshot(
                id, sales ? "SalesInvoice" : "PurchaseDocument",
                row.Id, row.Status.ToString(),
                row.AccountId, row.Debit, row.Credit));
        }
        return result;
    }

    public async Task<IReadOnlyList<FinancialControlLedgerSnapshot>> GetControlLedgerBalancesAsync(
        Guid organizationId, DateOnly cutoff, IReadOnlyList<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0) return [];

        return await (
            from line in dbContext.JournalLines.AsNoTracking()
            join entry in dbContext.JournalEntries.AsNoTracking()
                on new { line.JournalEntryId, line.OrganizationId }
                equals new { JournalEntryId = entry.Id, entry.OrganizationId }
            where line.OrganizationId == organizationId &&
                accountIds.Contains(line.AccountId) &&
                entry.PostingDate <= cutoff &&
                (entry.Status == JournalEntryStatus.Posted ||
                 entry.Status == JournalEntryStatus.Reversed)
            group line by line.AccountId into grouped
            select new FinancialControlLedgerSnapshot(
                grouped.Key,
                grouped.Sum(line => line.Debit),
                grouped.Sum(line => line.Credit))
        ).ToArrayAsync(cancellationToken);
    }
}
