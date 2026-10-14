namespace Fintrox.Contracts.Accounting;

/// <summary>
/// Current-state AR/AP open item, translated into the company's GL base currency.
/// Not a historic as-of snapshot of post-period cancellations.
/// </summary>
public sealed record FinancialOpenItemResponse(
    Guid DocumentId,
    string SourceType,
    Guid CounterpartyId,
    string? DocumentNumber,
    DateOnly DocumentDate,
    DateOnly DueDate,
    decimal DocumentGrossAmount,
    decimal AllocatedDocumentAmount,
    decimal OutstandingDocumentAmount,
    decimal OutstandingBaseAmount,
    Guid? ControlAccountId,
    bool Overdue,
    int DaysOverdue,
    string AgingBucket);

public sealed record FinancialCounterpartyOpenBalanceResponse(
    Guid CounterpartyId,
    string Type,
    decimal OutstandingBaseAmount,
    int OpenDocumentCount,
    int OverdueDocumentCount);

public sealed record FinancialControlAccountReconciliationResponse(
    Guid AccountId,
    string Type,
    decimal ExpectedOpenItemsBase,
    decimal ActualLedgerBalanceBase,
    decimal DifferenceBase,
    bool Balanced);

public sealed record FinancialOpenItemReconciliationIssue(
    string Code,
    string SourceType,
    Guid? DocumentId,
    Guid? PaymentId,
    string Description);

public sealed record FinancialOpenItemReconciliationResponse(
    Guid FiscalYearId,
    DateOnly CutoffDate,
    bool HistoricalAsOfSnapshotAvailable,
    bool AccountsReceivableReconciled,
    bool AccountsPayableReconciled,
    bool BankStatementsReconciled,
    bool FinancialSubsystemsFullyReconciled,
    decimal ReceivablesOpenBase,
    decimal PayablesOpenBase,
    IReadOnlyList<FinancialOpenItemResponse> OpenItems,
    IReadOnlyList<FinancialCounterpartyOpenBalanceResponse> CounterpartyBalances,
    IReadOnlyList<FinancialControlAccountReconciliationResponse> ControlAccounts,
    IReadOnlyList<FinancialOpenItemReconciliationIssue> Issues);
