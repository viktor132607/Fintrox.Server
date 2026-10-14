namespace Fintrox.Application.Accounting;

/// <summary>Posted base-currency debit/credit totals for a P&amp;L account in one fiscal year.</summary>
public sealed record ProfitLossAccountBalance(
    Guid AccountId,
    decimal Debit,
    decimal Credit);
