namespace Fintrox.Contracts.Accounting;

public sealed record FinancialSourceReconciliationIssue(
    string SourceType,
    Guid? SourceId,
    Guid? JournalEntryId,
    string Code,
    string Detail);

/// <summary>
/// Current-state sales/purchases/payments-to-GL integrity check.
/// Does NOT attest bank statements or historical as-of AR/AP balances.
/// </summary>
public sealed record FinancialSourceReconciliationResponse(
    Guid FiscalYearId,
    DateOnly FromDate,
    DateOnly ToDate,
    int SourceDocuments,
    int SourceJournals,
    int IssuesCount,
    bool SalesPostingIntegrityVerified,
    bool PurchasesPostingIntegrityVerified,
    bool PaymentsPostingIntegrityVerified,
    bool BankingReconciliationAvailable,
    bool FinancialSubsystemsFullyReconciled,
    IReadOnlyList<FinancialSourceReconciliationIssue> Issues);
