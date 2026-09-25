using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateJournalEntryRequest(
    DateOnly PostingDate,
    DateOnly DocumentDate,
    [property: Required, MaxLength(500)] string Description,
    [property: MaxLength(160)] string? ExternalReference);
