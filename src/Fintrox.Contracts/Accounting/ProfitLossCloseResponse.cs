namespace Fintrox.Contracts.Accounting;

/// <summary>Result of one atomic fiscal-year profit/loss closing operation.</summary>
public sealed record ProfitLossCloseResponse(
    Guid FiscalYearId,
    Guid? JournalEntryId,
    decimal NetIncome,
    bool AlreadyClosed);
