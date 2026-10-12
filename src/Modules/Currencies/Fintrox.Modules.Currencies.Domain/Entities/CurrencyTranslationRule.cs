namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyTranslationRule. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyTranslationRule : OrganizationEntity
{
    public CurrencyTranslationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string sourceCurrency,
        string targetCurrency,
        string rateBasis,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        SourceCurrency = DomainRules.Currency(sourceCurrency, nameof(sourceCurrency));
        TargetCurrency = DomainRules.Currency(targetCurrency, nameof(targetCurrency));
        RateBasis = DomainRules.Text(rateBasis, nameof(rateBasis));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public string RateBasis { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
