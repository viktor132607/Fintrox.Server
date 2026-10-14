namespace Fintrox.Contracts.Accounting;

/// <summary>
/// GL-only reconciliation: not a full AP/AR, bank, inventory or tax attestation.
/// </summary>
public sealed record FiscalCloseReconciliationResponse(
    Guid FiscalYearId,
    string FiscalYearStatus,
    decimal PostedDebitTotal,
    decimal PostedCreditTotal,
    decimal OpenProfitLossDebit,
    decimal OpenProfitLossCredit,
    int DraftJournalCount,
    bool LedgerBalanced,
    bool ProfitLossCleared,
    bool CalendarContiguous,
    bool PeriodsReadyForClose,
    bool ReadyForProfitLossClose,
    Guid? LatestClosingJournalEntryId);
