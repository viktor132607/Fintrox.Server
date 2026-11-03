namespace Fintrox.Contracts.Accounting;

/// <summary>
/// A fixed, tenant-owned, straight-line recognition schedule.
/// PrepaidExpense: Dr expense / Cr prepaid asset;
/// DeferredRevenue: Dr contract liability / Cr revenue;
/// AccruedExpense: Dr expense / Cr accrual liability;
/// AccruedRevenue: Dr accrued asset / Cr revenue.
/// Initial prepayment/deferral booking and final settlement remain separate.
/// </summary>
public sealed record CreateRecognitionScheduleRequest(
    string Kind, string Name, string Description,
    DateOnly FirstPostingDate, int Months, decimal TotalAmount,
    Guid DebitAccountId, Guid CreditAccountId);

public sealed record RecognitionScheduleResponse(
    Guid Id, string Kind, string Name, string Description,
    DateOnly FirstPostingDate, int Months, decimal TotalAmount,
    decimal MonthlyAmount, Guid DebitAccountId, Guid CreditAccountId);

public sealed record RecognitionOccurrenceResponse(
    int PeriodNumber, DateOnly PostingDate, decimal Amount,
    string Status, Guid? JournalEntryId);

public sealed record RecognitionScheduleDetailResponse(
    RecognitionScheduleResponse Schedule,
    IReadOnlyList<RecognitionOccurrenceResponse> Occurrences);

public sealed record GenerateRecognitionRequest(int PeriodNumber);

public sealed record RecognitionGenerationResponse(
    Guid ScheduleId, int PeriodNumber, Guid JournalEntryId,
    DateOnly PostingDate, decimal Amount, string JournalStatus,
    bool AlreadyGenerated);
