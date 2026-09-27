using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record CreateJournalLineRequest(
    Guid AccountId,
    [property: Range(typeof(decimal), "0", "999999999999999.9999")] decimal Debit,
    [property: Range(typeof(decimal), "0", "999999999999999.9999")] decimal Credit,
    [property: MaxLength(500)] string? Description);
