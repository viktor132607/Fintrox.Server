using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record ReverseJournalEntryRequest(
    DateOnly PostingDate,
    [property: Required, MaxLength(400)] string Reason);
