using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateAccountingPeriodRequest(
    [param: Range(1, 99)] int Number,
    [param: Required, MaxLength(80)] string Name,
    DateOnly StartDate,
    DateOnly EndDate);
