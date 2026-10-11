namespace Fintrox.Modules.Currencies.Domain.Entities;

public sealed class CurrencyRevaluation : OrganizationEntity
{
    public CurrencyRevaluation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        DateOnly asOfDate,
        string currency,
        decimal rate,
        string source)
        : base(id, organizationId, createdAtUtc)
    {
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Rate = DomainRules.Positive(rate, nameof(rate));
        Source = DomainRules.Text(source, nameof(source));
    }

    public DateOnly AsOfDate { get; }
    public string Currency { get; }
    public decimal Rate { get; }
    public string Source { get; }
}
