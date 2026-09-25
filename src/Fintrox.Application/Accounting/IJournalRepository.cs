using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public interface IJournalRepository
{
    Task<IReadOnlyList<JournalEntry>> ListEntriesAsync(
        Guid organizationId,
        DateOnly? fromPostingDate,
        DateOnly? toPostingDate,
        JournalEntryStatus? status,
        JournalEntrySource? source,
        CancellationToken cancellationToken);

    Task<JournalEntry?> GetEntryAsync(
        Guid organizationId,
        Guid journalEntryId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<JournalLine>> ListLinesAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken);

    Task<JournalLine?> GetLineAsync(
        Guid organizationId,
        Guid journalEntryId,
        Guid journalLineId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<int> GetNextLineNumberAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken);

    Task AddEntryAsync(
        JournalEntry entry,
        CancellationToken cancellationToken);

    Task AddLineAsync(
        JournalLine line,
        CancellationToken cancellationToken);

    void RemoveLine(JournalLine line);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
