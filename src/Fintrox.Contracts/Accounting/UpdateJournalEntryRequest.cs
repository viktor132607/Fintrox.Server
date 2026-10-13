using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateJournalEntryRequest(
    DateOnly PostingDate,
    DateOnly DocumentDate,
    [param: Required, MaxLength(500)] string Description,
    [param: MaxLength(160)] string? ExternalReference);
