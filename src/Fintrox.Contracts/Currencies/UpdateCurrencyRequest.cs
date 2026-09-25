using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Currencies;

public sealed record UpdateCurrencyRequest(
    [property: Required, StringLength(3, MinimumLength = 3)] string Code,
    [property: Required, MaxLength(120)] string Name,
    [property: MaxLength(12)] string? Symbol,
    [property: Range(0, 4)] int DecimalPlaces);
