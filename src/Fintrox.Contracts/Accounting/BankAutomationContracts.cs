using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

/// <summary>
/// UTF-8 CSV statement data in a JSON string, with explicit balance controls.
/// Columns: externalId,valueDate,signedAmount,description.
/// ISO dates yyyy-MM-dd and invariant dot-decimal values only.
/// </summary>
public sealed record ImportCsvBankStatementRequest(
    [param: Required, MinLength(1), MaxLength(120)] string ExternalId,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal OpeningBalance,
    decimal ClosingBalance,
    [param: Required, MinLength(1)] string Csv);

public sealed record BankMatchSuggestionResponse(
    Guid StatementLineId, Guid PaymentId,
    decimal SignedAmount, DateOnly ValueDate);

public sealed record BankMatchSuggestionsResponse(
    Guid StatementId,
    int UnmatchedStatementLines,
    int UnambiguousSuggestions,
    IReadOnlyList<BankMatchSuggestionResponse> Suggestions);

/// <summary>
/// Only one-to-one exact matches are committed; other lines stay unmatched.
/// A batch can have partial success if an individual candidate loses a race.
/// </summary>
public sealed record AutoMatchBankStatementResponse(
    Guid StatementId, int Proposed, int Matched,
    int AlreadyMatched, int Skipped,
    IReadOnlyList<Guid> SkippedLineIds);
