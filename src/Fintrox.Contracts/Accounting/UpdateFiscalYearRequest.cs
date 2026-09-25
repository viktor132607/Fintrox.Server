using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Accounting;

public sealed record UpdateFiscalYearRequest(
    [property: Required, MaxLength(80)] string Name,
    DateOnly StartDate,
    DateOnly EndDate);
