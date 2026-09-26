namespace Fintrox.Contracts.Accounting;

public sealed record UpdateAutoPostingRuleRequest(
    string Component,
    string MatchKind,
    string? MatchValue,
    Guid AccountId);
