using Fintrox.Application.Sales;
using Fintrox.Domain.Sales;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Sales;

public sealed class SalesInvoiceRepository(
    FintroxDbContext dbContext) : ISalesInvoiceRepository
{
    public async Task<IReadOnlyList<SalesInvoice>> ListInvoicesAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        SalesInvoiceStatus? status,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        IQueryable<SalesInvoice> query = dbContext.SalesInvoices
            .AsNoTracking()
            .Where(invoice => invoice.OrganizationId == organizationId);

        if (fromDate.HasValue)
        {
            query = query.Where(invoice =>
                invoice.InvoiceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(invoice =>
                invoice.InvoiceDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(invoice =>
                invoice.Status == status.Value);
        }

        if (counterpartyId.HasValue)
        {
            query = query.Where(invoice =>
                invoice.CounterpartyId == counterpartyId.Value);
        }

        return await query
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ThenByDescending(invoice => invoice.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SalesInvoice?> GetInvoiceAsync(
        Guid organizationId,
        Guid invoiceId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<SalesInvoice> query = dbContext.SalesInvoices;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            invoice =>
                invoice.OrganizationId == organizationId &&
                invoice.Id == invoiceId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<SalesInvoiceLine>> ListLinesAsync(
        Guid organizationId,
        Guid invoiceId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<SalesInvoiceLine> query = dbContext.SalesInvoiceLines
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.SalesInvoiceId == invoiceId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(line => line.LineNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SalesInvoiceLine?> GetLineAsync(
        Guid organizationId,
        Guid invoiceId,
        Guid lineId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<SalesInvoiceLine> query = dbContext.SalesInvoiceLines;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            line =>
                line.OrganizationId == organizationId &&
                line.SalesInvoiceId == invoiceId &&
                line.Id == lineId,
            cancellationToken);
    }

    public async Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var maxLineNumber = await dbContext.SalesInvoiceLines
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.SalesInvoiceId == invoiceId)
            .Select(line => (int?)line.LineNumber)
            .MaxAsync(cancellationToken);

        return (maxLineNumber ?? 0) + 1;
    }

    public async Task<long> AllocateInvoiceSequenceAsync(
        Guid organizationId,
        int calendarYear,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Sales invoice numbers must be allocated inside a database transaction.");
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO sales.sales_invoice_number_sequences
                (organization_id, calendar_year, last_number)
            VALUES
                ({organizationId}, {calendarYear}, 1)
            ON CONFLICT (organization_id, calendar_year)
            DO UPDATE SET
                last_number = sales.sales_invoice_number_sequences.last_number + 1
            """,
            cancellationToken);

        return await dbContext.SalesInvoiceNumberSequences
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.CalendarYear == calendarYear)
            .Select(sequence => sequence.LastNumber)
            .SingleAsync(cancellationToken);
    }

    public async Task AddInvoiceAsync(
        SalesInvoice invoice,
        CancellationToken cancellationToken)
    {
        await dbContext.SalesInvoices.AddAsync(
            invoice,
            cancellationToken);
    }

    public async Task AddLineAsync(
        SalesInvoiceLine line,
        CancellationToken cancellationToken)
    {
        await dbContext.SalesInvoiceLines.AddAsync(
            line,
            cancellationToken);
    }

    public void RemoveLine(SalesInvoiceLine line)
    {
        dbContext.SalesInvoiceLines.Remove(line);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
