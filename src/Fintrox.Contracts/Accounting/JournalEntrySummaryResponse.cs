namespace Fintrox.Contracts.Accounting;

public sealed record JournalEntrySummaryResponse(
    Guid Id,
    string? Number,
    DateOnly PostingDate,
    DateOnly DocumentDate,
    string Description,
    string Status,
    string Source,
    string? ExternalReference,
    Guid FiscalPeriodId,
    DateTimeOffset? PostedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
