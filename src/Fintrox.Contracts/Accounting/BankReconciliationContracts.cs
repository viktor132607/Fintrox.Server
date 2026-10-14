using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record CreateBankAccountRequest(
    [param: Required, MinLength(8), MaxLength(34)] string Iban,
    [param: Required, MinLength(1), MaxLength(160)] string Name,
    [param: Required, StringLength(3, MinimumLength = 3)] string CurrencyCode,
    Guid LedgerAccountId);

public sealed record BankAccountResponse(
    Guid Id, string Iban, string Name, string CurrencyCode,
    Guid LedgerAccountId);

public sealed record BankStatementLineImportRequest(
    [param: Required, MinLength(1), MaxLength(120)] string ExternalId,
    DateOnly ValueDate,
    decimal SignedAmount,
    [param: Required, MinLength(1), MaxLength(500)] string Description);

public sealed record ImportBankStatementRequest(
    [param: Required, MinLength(1), MaxLength(120)] string ExternalId,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal OpeningBalance,
    decimal ClosingBalance,
    [param: Required, MinLength(1)] IReadOnlyList<BankStatementLineImportRequest> Lines);

public sealed record BankStatementImportResponse(
    Guid Id, Guid BankAccountId, string ExternalId,
    DateOnly FromDate, DateOnly ToDate, int LineCount,
    decimal OpeningBalance, decimal ClosingBalance,
    bool AlreadyImported);

public sealed record MatchBankStatementLineRequest(Guid PaymentId);

public sealed record BankStatementLineResponse(
    Guid Id, string ExternalId, DateOnly ValueDate,
    decimal SignedAmount, string Description,
    Guid? MatchedPaymentId, Guid? FeeJournalEntryId,
    Guid? ForeignSettlementJournalEntryId,
    Guid? CorrectionReversalJournalEntryId);

public sealed record BankStatementResponse(
    Guid Id, Guid BankAccountId, string ExternalId,
    DateOnly FromDate, DateOnly ToDate,
    decimal OpeningBalance, decimal ClosingBalance,
    IReadOnlyList<BankStatementLineResponse> Lines);

/// <summary>Explicit approved bank expense mapping and replay precondition.</summary>
public sealed record PostBankFeeRequest(
    Guid ExpenseAccountId,
    decimal ExpectedSignedAmount,
    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(250, MinimumLength = 10)]
    string Reason);

public sealed record BankFeePostingResponse(
    Guid StatementId, Guid StatementLineId,
    Guid JournalEntryId, Guid ExpenseAccountId,
    decimal FeeAmount, string Reason, bool AlreadyPosted);

/// <summary>
/// Debit/credit settlement against a real foreign currency bank line.
/// ExchangeRate.Rate means quote units per one base-currency unit.
/// ClearingBaseAmount is the ledger carrying value of the AR/AP amount paid.
/// </summary>
public sealed record PostForeignBankSettlementRequest(
    Guid ExchangeRateId,
    Guid ClearingAccountId,
    decimal ClearingBaseAmount,
    Guid FxGainAccountId,
    Guid FxLossAccountId,
    decimal ExpectedSignedForeignAmount);

public sealed record ForeignBankSettlementResponse(
    Guid StatementId, Guid StatementLineId, Guid JournalEntryId,
    string CurrencyCode, decimal NativeAmount,
    decimal BookedRate, decimal BankBaseAmount,
    decimal ClearingBaseAmount, decimal RealizedGainBase,
    decimal RealizedLossBase, bool AlreadyPosted);

/// <summary>
/// Explicit year/period end bank FX valuation. Two expected balances protect
/// against changes to the original statement and cumulative base-currency GL.
/// Quote rate is foreign units per one organization base-currency unit.
/// </summary>
public sealed record PostForeignBankRevaluationRequest(
    Guid ExchangeRateId,
    Guid FxGainAccountId,
    Guid FxLossAccountId,
    decimal ExpectedNativeClosingBalance,
    decimal ExpectedLedgerBaseBalance);

public sealed record ForeignBankRevaluationResponse(
    Guid StatementId, Guid BankAccountId, Guid? JournalEntryId,
    DateOnly CutoffDate, string CurrencyCode,
    decimal NativeClosingBalance, decimal ApprovedRate,
    decimal PreviousLedgerBaseBalance, decimal TargetBaseBalance,
    decimal SignedAdjustmentBase, decimal UnrealizedGainBase,
    decimal UnrealizedLossBase, bool AlreadyRevalued);

/// <summary>
/// Explicit compensating correction of a posted bank fee or realized FX
/// settlement. This does not alter or delete the imported statement line.
/// </summary>
public sealed record CorrectBankPostingRequest(
    string SourceKind,
    Guid OriginalJournalEntryId,
    DateOnly CorrectionPostingDate,
    string Reason);

public sealed record BankPostingCorrectionResponse(
    Guid StatementId,
    Guid StatementLineId,
    string SourceKind,
    Guid OriginalJournalEntryId,
    Guid ReversalJournalEntryId,
    DateOnly CorrectionPostingDate,
    Guid CorrectedByUserId,
    bool AlreadyCorrected);

public sealed record BankStatementMatchResponse(
    Guid StatementId, Guid LineId, Guid PaymentId, bool AlreadyMatched);

public sealed record BankReconciliationResponse(
    Guid StatementId, Guid BankAccountId, string CurrencyCode,
    DateOnly ToDate, decimal StatementOpeningBalance,
    decimal StatementClosingBalance, decimal ExpectedStatementClosingBalance,
    decimal GeneralLedgerClosingBalance, decimal? StatementVsLedgerDifference,
    int StatementLines, int MatchedLines, int UnmatchedLines,
    int UnclearedBookPayments,
    bool StatementsBalanced, bool StatementLinesAllMatched,
    bool BankAndLedgerBalanced, bool FullyReconciled,
    IReadOnlyList<Guid> UnmatchedLineIds,
    IReadOnlyList<Guid> UnclearedPaymentIds, int PostedFeeLines,
    int PostedForeignSettlementLines, bool BankAndLedgerCurrencyComparable);

public sealed class BankReconciliationConflictException(string message) : Exception(message);
