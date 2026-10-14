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

    public async Task LockJournalForPostingAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken)
    {
        EnsurePostingTransaction();

        // SELECT FOR UPDATE (not FOR SHARE) serializes the entire posting
        // lifecycle. A losing concurrent request must re-read the new status.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.journal_entries
            WHERE organization_id = {organizationId} AND id = {journalEntryId}
            FOR UPDATE
            """,
            cancellationToken);
    }

    public async Task LockPostingCalendarAsync(
        Guid organizationId,
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        EnsurePostingTransaction();

        // The fiscal-year lock must precede the period lock on every posting
        // path. Closing a year/period acquires the same PostgreSQL row locks
        // during UPDATE. Shared locks allow simultaneous postings but block
        // status writes until all financial postings in progress commit.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.fiscal_years
            WHERE organization_id = {organizationId} AND id = {fiscalYearId}
            FOR SHARE
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.accounting_periods
            WHERE organization_id = {organizationId} AND id = {periodId}
            FOR SHARE
            """,
            cancellationToken);
    }

    public async Task LockFiscalYearForOpeningBalancesAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        EnsurePostingTransaction();

        // An exclusive year lock serializes the first-book import against
        // ordinary postings (which hold FOR SHARE on the same year).
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            SELECT id FROM accounting.fiscal_years
            WHERE organization_id = {organizationId} AND id = {fiscalYearId}
            FOR UPDATE
            """,
            cancellationToken);
    }

    public Task<bool> HasPostedEntriesForFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        return dbContext.JournalEntries.AsNoTracking().AnyAsync(
            entry => entry.OrganizationId == organizationId &&
                (entry.Status == JournalEntryStatus.Posted ||
                 entry.Status == JournalEntryStatus.Reversed) &&
                dbContext.AccountingPeriods.Any(period =>
                    period.OrganizationId == organizationId &&
                    period.Id == entry.FiscalPeriodId &&
                    period.FiscalYearId == fiscalYearId),
            cancellationToken);
    }

    private void EnsurePostingTransaction()
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Posting row locks require an active database transaction.");
        }
    }

    public async Task<IReadOnlyList<JournalEntry>> ListYearEndClosingEntriesAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken)
    {
        // v1 is retained for the first close, :r2/:r3 for correction cycles.
        // Match the precise prefix including delimiter, not a different ID.
        var prefix = $"profit-loss-close:v1:{fiscalYearId:N}";
        return await dbContext.JournalEntries.AsNoTracking()
            .Where(entry => entry.OrganizationId == organizationId &&
                entry.Source == JournalEntrySource.System &&
                (entry.ExternalReference == prefix ||
                 (entry.ExternalReference != null &&
                  entry.ExternalReference.StartsWith(prefix + ":r"))))
            .OrderBy(entry => entry.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<FiscalLedgerTotals> GetFiscalLedgerTotalsAsync(
        Guid organizationId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from line in dbContext.JournalLines.AsNoTracking()
            join entry in dbContext.JournalEntries.AsNoTracking()
                on new { line.JournalEntryId, line.OrganizationId }
                equals new { JournalEntryId = entry.Id, entry.OrganizationId }
            where line.OrganizationId == organizationId &&
                entry.PostingDate >= fromDate &&
                entry.PostingDate <= toDate &&
                (entry.Status == JournalEntryStatus.Posted ||
                 entry.Status == JournalEntryStatus.Reversed)
            group line by 1 into totals
            select new FiscalLedgerTotals(
                totals.Sum(line => line.Debit),
                totals.Sum(line => line.Credit),
                totals.Count())
        ).SingleOrDefaultAsync(cancellationToken);
        return rows ?? new FiscalLedgerTotals(0m, 0m, 0);
    }

    public async Task<IReadOnlyList<ProfitLossAccountBalance>> GetProfitLossBalancesAsync(
        Guid organizationId,
        DateOnly fiscalYearStart,
        DateOnly fiscalYearEnd,
        CancellationToken cancellationToken)
    {
        return await (
            from line in dbContext.JournalLines.AsNoTracking()
            join entry in dbContext.JournalEntries.AsNoTracking()
                on new { line.JournalEntryId, line.OrganizationId }
                equals new { JournalEntryId = entry.Id, entry.OrganizationId }
            join account in dbContext.Accounts.AsNoTracking()
                on new { line.AccountId, line.OrganizationId }
                equals new { AccountId = account.Id, account.OrganizationId }
            where line.OrganizationId == organizationId &&
                entry.PostingDate >= fiscalYearStart &&
                entry.PostingDate <= fiscalYearEnd &&
                (entry.Status == JournalEntryStatus.Posted ||
                 entry.Status == JournalEntryStatus.Reversed) &&
                (account.Type == AccountType.Revenue || account.Type == AccountType.Expense)
            group line by line.AccountId into grouped
            select new ProfitLossAccountBalance(
                grouped.Key,
                grouped.Sum(line => line.Debit),
                grouped.Sum(line => line.Credit))
        ).ToArrayAsync(cancellationToken);
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
