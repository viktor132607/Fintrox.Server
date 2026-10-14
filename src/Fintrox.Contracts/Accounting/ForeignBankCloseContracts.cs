namespace Fintrox.Contracts.Accounting;

/// <summary>
/// Bridge between the native bank statement and the base-currency GL after
/// an independently selected cutoff exchange rate and recorded revaluation.
/// </summary>
public sealed record ForeignBankCloseEvidenceResponse(
    Guid StatementId, Guid BankAccountId, DateOnly CutoffDate,
    string CurrencyCode, decimal NativeClosingBalance,
    decimal ApprovedCutoffRate, decimal RevaluedTargetBase,
    decimal ActualPostedLedgerBase, decimal BaseDifference,
    bool NativeStatementChainValid, bool AllNativeLinesPosted,
    bool RevaluationJournalValid, bool NoUnrecognizedLedgerEntries,
    bool InternallyReconciled, bool Prepared, bool Approved,
    Guid? PreparedByUserId, Guid? ApprovedByUserId,
    IReadOnlyList<string> Issues, int VersionNumber = 1);

public sealed record PrepareForeignBankCloseRequest(string ReviewNote);

public sealed record ApproveForeignBankCloseRequest(string ApprovalNote);

public sealed record ForeignBankCloseDecisionResponse(
    Guid StatementId, Guid BankAccountId, Guid PreparationId,
    Guid PreparedByUserId, Guid? ApprovedByUserId,
    string EvidenceSha256, bool Approved, bool AlreadyRecorded,
    int VersionNumber = 1);

public sealed record AmendForeignBankRevaluationRequest(
    int ExpectedCurrentVersion,
    Guid? ExpectedCurrentJournalEntryId,
    Guid ExchangeRateId,
    Guid FxGainAccountId,
    Guid FxLossAccountId,
    decimal ExpectedNativeClosingBalance,
    decimal ExpectedLedgerBaseBalance,
    string Reason);

public sealed record ForeignBankRevaluationAmendmentResponse(
    Guid StatementId, Guid BankAccountId, int VersionNumber,
    Guid? PreviousJournalEntryId, Guid? ReversalJournalEntryId,
    Guid? ReplacementJournalEntryId, decimal OriginalLedgerBase,
    decimal PreviousTargetBase, decimal ReplacementTargetBase,
    decimal ReplacementAdjustmentBase, decimal ApprovedRate,
    Guid CorrectedByUserId, bool AlreadyAmended);
