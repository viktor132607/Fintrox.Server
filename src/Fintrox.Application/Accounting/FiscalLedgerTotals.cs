namespace Fintrox.Application.Accounting;

/// <summary>Year-filtered base-currency GL totals, including reversal lines.</summary>
public sealed record FiscalLedgerTotals(decimal Debit, decimal Credit, int LineCount);
