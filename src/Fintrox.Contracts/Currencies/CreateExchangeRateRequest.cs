using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Currencies;

public sealed record CreateExchangeRateRequest(
    Guid BaseCurrencyId,
    Guid QuoteCurrencyId,
    DateOnly EffectiveDate,
    [property: Range(typeof(decimal), "0.0000000001", "999999999999.9999999999")] decimal Rate,
    [property: MaxLength(120)] string? Source);
