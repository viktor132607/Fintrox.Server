namespace Fintrox.Contracts.Accounting;

/// <summary>Explicit destination equity account for the fiscal result.</summary>
public sealed record CloseProfitLossRequest(Guid RetainedEarningsAccountId);
