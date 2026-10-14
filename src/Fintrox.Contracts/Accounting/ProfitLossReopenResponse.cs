namespace Fintrox.Contracts.Accounting;

public sealed record ProfitLossReopenResponse(
    Guid FiscalYearId,
    Guid ClosingJournalEntryId,
    Guid ReversalJournalEntryId,
    bool AlreadyReopened);
