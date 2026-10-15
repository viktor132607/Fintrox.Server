namespace Fintrox.Contracts.Accounting;

/// <summary>Monthly template (day 1–28) with fixed balanced journal lines.</summary>
public sealed record CreateRecurringJournalTemplateRequest(
    string Name, string Description,
    DateOnly FirstPostingDate, DateOnly? LastPostingDate,
    IReadOnlyList<CreateJournalLineRequest> Lines);

public sealed record RecurringJournalTemplateResponse(
    Guid Id, string Name, string Description,
    DateOnly FirstPostingDate, DateOnly? LastPostingDate,
    int DayOfMonth, IReadOnlyList<CreateJournalLineRequest> Lines);

public sealed record GenerateRecurringJournalRequest(DateOnly PostingDate);

public sealed record RecurringJournalGenerationResponse(
    Guid TemplateId, Guid JournalEntryId, DateOnly PostingDate,
    string JournalStatus, bool AlreadyGenerated);

/// <summary>Read-only monthly occurrence status projection.</summary>
public sealed record RecurringJournalOccurrenceResponse(
    Guid TemplateId, DateOnly PostingDate, string OccurrenceStatus,
    Guid? JournalEntryId, string? JournalStatus);

public sealed record RecurringJournalScheduleResponse(
    Guid TemplateId, DateOnly FromDate, DateOnly ToDate, DateOnly AsOfDate,
    IReadOnlyList<RecurringJournalOccurrenceResponse> Occurrences);
