namespace Fintrox.Contracts.Currencies;

public sealed record ExchangeRateResponse(
    Guid Id,
    Guid OrganizationId,
    Guid BaseCurrencyId,
    string BaseCurrencyCode,
    Guid QuoteCurrencyId,
    string QuoteCurrencyCode,
    DateOnly EffectiveDate,
    decimal Rate,
    string? Source,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
