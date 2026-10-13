using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateJournalLineRequest(
    Guid AccountId,
    [param: Range(typeof(decimal), "0", "999999999999999.9999")] decimal Debit,
    [param: Range(typeof(decimal), "0", "999999999999999.9999")] decimal Credit,
    [param: MaxLength(500)] string? Description);
