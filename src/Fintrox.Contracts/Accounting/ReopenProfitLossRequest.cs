using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

/// <summary>Reason recorded on the immutable compensating journal.</summary>
public sealed record ReopenProfitLossRequest(
    Guid ExpectedClosingJournalEntryId,
    [param: Required, MinLength(15), MaxLength(250)] string Reason);
