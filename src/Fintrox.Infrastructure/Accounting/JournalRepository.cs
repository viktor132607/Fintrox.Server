using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

public sealed class JournalRepository(
    FintroxDbContext dbContext) : IJournalRepository
{
    public async Task<IReadOnlyList<JournalEntry>> ListEntriesAsync(
        Guid organizationId,
        DateOnly? fromPostingDate,
        DateOnly? toPostingDate,
        JournalEntryStatus? status,
        JournalEntrySource? source,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalEntry> query = dbContext.JournalEntries
            .AsNoTracking()
            .Where(entry => entry.OrganizationId == organizationId);

        if (fromPostingDate.HasValue)
        {
            query = query.Where(
                entry => entry.PostingDate >= fromPostingDate.Value);
        }

        if (toPostingDate.HasValue)
        {
            query = query.Where(
                entry => entry.PostingDate <= toPostingDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(entry => entry.Status == status.Value);
        }

        if (source.HasValue)
        {
            query = query.Where(entry => entry.Source == source.Value);
        }

        return await query
            .OrderByDescending(entry => entry.PostingDate)
            .ThenByDescending(entry => entry.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<JournalEntry?> GetEntryAsync(
        Guid organizationId,
        Guid journalEntryId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalEntry> query = dbContext.JournalEntries;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            entry =>
                entry.OrganizationId == organizationId &&
                entry.Id == journalEntryId,
            cancellationToken);
    }

    public async Task<JournalEntry?> GetSystemEntryByExternalReferenceAsync(
        Guid organizationId,
        string externalReference,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalEntry> query = dbContext.JournalEntries;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            entry =>
                entry.OrganizationId == organizationId &&
                entry.Source == JournalEntrySource.System &&
                entry.ExternalReference == externalReference,
            cancellationToken);
    }

    public async Task<IReadOnlyList<JournalLine>> ListLinesAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        return await dbContext.JournalLines
            .AsNoTracking()
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.JournalEntryId == journalEntryId)
            .OrderBy(line => line.LineNumber)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<JournalLine?> GetLineAsync(
        Guid organizationId,
        Guid journalEntryId,
        Guid journalLineId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<JournalLine> query = dbContext.JournalLines;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            line =>
                line.OrganizationId == organizationId &&
                line.JournalEntryId == journalEntryId &&
                line.Id == journalLineId,
            cancellationToken);
    }

    public async Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        var maxLineNumber = await dbContext.JournalLines
            .Where(line =>
                line.OrganizationId == organizationId &&
                line.JournalEntryId == journalEntryId)
            .Select(line => (int?)line.LineNumber)
            .MaxAsync(cancellationToken);

        return (maxLineNumber ?? 0) + 1;
    }

    public async Task<long> AllocatePostingSequenceAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Journal posting numbers must be allocated inside a database transaction.");
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO accounting.journal_number_sequences
                (organization_id, fiscal_year_id, last_number)
            VALUES
                ({organizationId}, {fiscalYearId}, 1)
            ON CONFLICT (organization_id, fiscal_year_id)
            DO UPDATE SET
                last_number = accounting.journal_number_sequences.last_number + 1
            """,
            cancellationToken);

        return await dbContext.JournalNumberSequences
            .AsNoTracking()
            .Where(sequence =>
                sequence.OrganizationId == organizationId &&
                sequence.FiscalYearId == fiscalYearId)
            .Select(sequence => sequence.LastNumber)
            .SingleAsync(cancellationToken);
    }

    public async Task AddEntryAsync(
        JournalEntry entry,
        CancellationToken cancellationToken)
    {
        await dbContext.JournalEntries.AddAsync(entry, cancellationToken);
    }

    public async Task AddLineAsync(
        JournalLine line,
        CancellationToken cancellationToken)
    {
        await dbContext.JournalLines.AddAsync(line, cancellationToken);
    }

    public void RemoveLine(JournalLine line)
    {
        dbContext.JournalLines.Remove(line);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
