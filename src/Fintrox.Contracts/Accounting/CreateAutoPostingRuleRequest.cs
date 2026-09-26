namespace Fintrox.Contracts.Accounting;

public sealed record CreateAutoPostingRuleRequest(
    string Component,
    string MatchKind,
    string? MatchValue,
    Guid AccountId);
