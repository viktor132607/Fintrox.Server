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
    Guid? MatchedPaymentId, Guid? FeeJournalEntryId);

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

public sealed record BankStatementMatchResponse(
    Guid StatementId, Guid LineId, Guid PaymentId, bool AlreadyMatched);

public sealed record BankReconciliationResponse(
    Guid StatementId, Guid BankAccountId, string CurrencyCode,
    DateOnly ToDate, decimal StatementOpeningBalance,
    decimal StatementClosingBalance, decimal ExpectedStatementClosingBalance,
    decimal GeneralLedgerClosingBalance, decimal StatementVsLedgerDifference,
    int StatementLines, int MatchedLines, int UnmatchedLines,
    int UnclearedBookPayments,
    bool StatementsBalanced, bool StatementLinesAllMatched,
    bool BankAndLedgerBalanced, bool FullyReconciled,
    IReadOnlyList<Guid> UnmatchedLineIds,
    IReadOnlyList<Guid> UnclearedPaymentIds, int PostedFeeLines);

public sealed class BankReconciliationConflictException(string message) : Exception(message);
