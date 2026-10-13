using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Currencies;

public sealed record UpdateCurrencyRequest(
    [param: Required, StringLength(3, MinimumLength = 3)] string Code,
    [param: Required, MaxLength(120)] string Name,
    [param: MaxLength(12)] string? Symbol,
    [param: Range(0, 4)] int DecimalPlaces);
