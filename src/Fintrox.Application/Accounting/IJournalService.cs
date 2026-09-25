using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IJournalService
{
    Task<IReadOnlyList<JournalEntrySummaryResponse>> ListAsync(
        DateOnly? fromPostingDate,
        DateOnly? toPostingDate,
        string? status,
        string? source,
        CancellationToken cancellationToken);

    Task<JournalEntryResponse?> GetAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken);

    Task<JournalEntryResponse> CreateAsync(
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken);

    Task<JournalEntryResponse?> UpdateAsync(
        Guid journalEntryId,
        UpdateJournalEntryRequest request,
        CancellationToken cancellationToken);

    Task<JournalLineResponse?> AddLineAsync(
        Guid journalEntryId,
        CreateJournalLineRequest request,
        CancellationToken cancellationToken);

    Task<JournalLineResponse?> UpdateLineAsync(
        Guid journalEntryId,
        Guid journalLineId,
        UpdateJournalLineRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteLineAsync(
        Guid journalEntryId,
        Guid journalLineId,
        CancellationToken cancellationToken);

    Task<JournalEntryResponse?> PostAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken);

    Task<JournalEntryResponse?> ReverseAsync(
        Guid journalEntryId,
        ReverseJournalEntryRequest request,
        CancellationToken cancellationToken);
}
