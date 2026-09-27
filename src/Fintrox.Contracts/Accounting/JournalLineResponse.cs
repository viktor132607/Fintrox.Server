namespace Fintrox.Contracts.Accounting;

public sealed record JournalLineResponse(
    Guid Id,
    int LineNumber,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string? Description,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
