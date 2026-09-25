namespace Fintrox.Contracts.Currencies;

public sealed record CurrencyResponse(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string? Symbol,
    int DecimalPlaces,
    bool IsBaseCurrency,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset UpdatedAtUtc,
    Guid? UpdatedByUserId);
