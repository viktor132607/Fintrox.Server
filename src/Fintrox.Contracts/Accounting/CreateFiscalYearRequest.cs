using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record CreateFiscalYearRequest(
    [property: Required, MaxLength(80)] string Name,
    DateOnly StartDate,
    DateOnly EndDate);
