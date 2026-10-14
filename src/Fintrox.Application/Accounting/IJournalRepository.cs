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

    Task<JournalEntry?> GetSystemEntryByExternalReferenceAsync(
        Guid organizationId,
        string externalReference,
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

    // Called only inside the posting/reversal transaction to serialize
    // competing transitions of one journal entry across API instances.
    Task LockJournalForPostingAsync(
        Guid organizationId,
        Guid journalEntryId,
        CancellationToken cancellationToken);

    // Locks fiscal year before period in a stable order. This coordinates
    // post/reversal with concurrent period and year status changes.
    Task LockPostingCalendarAsync(
        Guid organizationId,
        Guid fiscalYearId,
        Guid periodId,
        CancellationToken cancellationToken);

    Task LockFiscalYearForOpeningBalancesAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<bool> HasPostedEntriesForFiscalYearAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<JournalEntry>> ListYearEndClosingEntriesAsync(
        Guid organizationId,
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<FiscalLedgerTotals> GetFiscalLedgerTotalsAsync(
        Guid organizationId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProfitLossAccountBalance>> GetProfitLossBalancesAsync(
        Guid organizationId,
        DateOnly fiscalYearStart,
        DateOnly fiscalYearEnd,
        CancellationToken cancellationToken);

    Task<long> AllocatePostingSequenceAsync(
        Guid organizationId,
        Guid fiscalYearId,
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
