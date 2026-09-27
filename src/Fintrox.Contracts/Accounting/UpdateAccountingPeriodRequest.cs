using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateAccountingPeriodRequest(
    [property: Range(1, 99)] int Number,
    [property: Required, MaxLength(80)] string Name,
    DateOnly StartDate,
    DateOnly EndDate);
