namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyPair. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyPair : OrganizationEntity
{
    public CurrencyPair(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string baseCurrency,
        string quoteCurrency,
        decimal minimumRate,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        BaseCurrency = DomainRules.Currency(baseCurrency, nameof(baseCurrency));
        QuoteCurrency = DomainRules.Currency(quoteCurrency, nameof(quoteCurrency));
        MinimumRate = DomainRules.Positive(minimumRate, nameof(minimumRate));
        IsEnabled = isEnabled;
    }

    public string BaseCurrency { get; }
    public string QuoteCurrency { get; }
    public decimal MinimumRate { get; }
    public bool IsEnabled { get; }
}
