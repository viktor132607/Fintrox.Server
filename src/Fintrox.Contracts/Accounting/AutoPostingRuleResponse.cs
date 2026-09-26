namespace Fintrox.Contracts.Accounting;

public sealed record AutoPostingRuleResponse(
    Guid Id,
    Guid OrganizationId,
    string Component,
    string MatchKind,
    string? MatchValue,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
