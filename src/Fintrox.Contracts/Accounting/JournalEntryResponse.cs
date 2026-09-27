namespace Fintrox.Contracts.Accounting;

public sealed record JournalEntryResponse(
    Guid Id,
    Guid OrganizationId,
    string? Number,
    DateOnly PostingDate,
    DateOnly DocumentDate,
    string Description,
    string Status,
    string Source,
    string? ExternalReference,
    Guid FiscalPeriodId,
    DateTimeOffset? PostedAtUtc,
    decimal DebitTotal,
    decimal CreditTotal,
    bool IsBalanced,
    IReadOnlyList<JournalLineResponse> Lines,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
