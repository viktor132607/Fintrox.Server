using Fintrox.Application.Purchases;
using Fintrox.Domain.Purchases;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Purchases;

public sealed class PurchaseDocumentRepository(
    FintroxDbContext dbContext) : IPurchaseDocumentRepository
{
    public async Task<IReadOnlyList<PurchaseDocument>> ListAsync(
        Guid organizationId,
        DateOnly? fromDate,
        DateOnly? toDate,
        PurchaseDocumentStatus? status,
        PurchaseDocumentType? type,
        Guid? counterpartyId,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseDocument> query = dbContext.PurchaseDocuments
            .AsNoTracking()
            .Where(document => document.OrganizationId == organizationId);

        if (fromDate.HasValue)
        {
            query = query.Where(document =>
                document.DocumentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(document =>
                document.DocumentDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(document =>
                document.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(document =>
                document.Type == type.Value);
        }

        if (counterpartyId.HasValue)
        {
            query = query.Where(document =>
                document.CounterpartyId == counterpartyId.Value);
        }

        return await query
            .OrderByDescending(document => document.DocumentDate)
            .ThenByDescending(document => document.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PurchaseDocument?> GetAsync(
        Guid organizationId,
        Guid documentId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseDocument> query = dbContext.PurchaseDocuments;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            document =>
                document.OrganizationId == organizationId &&
                document.Id == documentId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseDocumentLine>> ListLinesAsync(
        Guid organizationId,
        Guid documentId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseDocumentLine> query = dbContext.PurchaseDocumentLines
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.PurchaseDocumentId == documentId);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query
            .OrderBy(line => line.LineNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<PurchaseDocumentLine?> GetLineAsync(
        Guid organizationId,
        Guid documentId,
        Guid lineId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<PurchaseDocumentLine> query = dbContext.PurchaseDocumentLines;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            line =>
                line.OrganizationId == organizationId &&
                line.PurchaseDocumentId == documentId &&
                line.Id == lineId,
            cancellationToken);
    }

    public async Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var max = await dbContext.PurchaseDocumentLines
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.PurchaseDocumentId == documentId)
            .Select(line => (int?)line.LineNumber)
            .MaxAsync(cancellationToken);

        return (max ?? 0) + 1;
    }

    public Task<bool> SupplierDocumentNumberExistsAsync(
        Guid organizationId,
        Guid counterpartyId,
        string supplierDocumentNumber,
        Guid? excludingDocumentId,
        CancellationToken cancellationToken)
    {
        return dbContext.PurchaseDocuments.AnyAsync(
            document =>
                document.OrganizationId == organizationId &&
                document.CounterpartyId == counterpartyId &&
                document.SupplierDocumentNumber == supplierDocumentNumber &&
                (!excludingDocumentId.HasValue ||
                 document.Id != excludingDocumentId.Value),
            cancellationToken);
    }

    public async Task<long> AllocateInternalSequenceAsync(
        Guid organizationId,
        int calendarYear,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Purchase document numbers must be allocated inside a database transaction.");
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO purchases.purchase_document_number_sequences
                (organization_id, calendar_year, last_number)
            VALUES
                ({organizationId}, {calendarYear}, 1)
            ON CONFLICT (organization_id, calendar_year)
            DO UPDATE SET
                last_number = purchases.purchase_document_number_sequences.last_number + 1
            """,
            cancellationToken);

        return await dbContext.PurchaseDocumentNumberSequences
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.CalendarYear == calendarYear)
            .Select(sequence => sequence.LastNumber)
            .SingleAsync(cancellationToken);
    }

    public async Task AddAsync(
        PurchaseDocument document,
        CancellationToken cancellationToken)
    {
        await dbContext.PurchaseDocuments.AddAsync(
            document,
            cancellationToken);
    }

    public async Task AddLineAsync(
        PurchaseDocumentLine line,
        CancellationToken cancellationToken)
    {
        await dbContext.PurchaseDocumentLines.AddAsync(
            line,
            cancellationToken);
    }

    public void RemoveLine(PurchaseDocumentLine line)
    {
        dbContext.PurchaseDocumentLines.Remove(line);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
