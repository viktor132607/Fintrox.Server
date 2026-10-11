namespace Fintrox.Modules.Currencies.Domain.Entities;

public sealed class CurrencyRoundingPolicy : OrganizationEntity
{
    public CurrencyRoundingPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string currency,
        int decimalPlaces)
        : base(id, organizationId, createdAtUtc)
    {
        Currency = DomainRules.Currency(currency, nameof(currency));
        DecimalPlaces = DomainRules.Scale(decimalPlaces, nameof(decimalPlaces));
    }

    public string Currency { get; }
    public int DecimalPlaces { get; }
}
