namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRateTolerance. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRateTolerance : OrganizationEntity
{
    public CurrencyRateTolerance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string currencyPairCode,
        decimal maximumVariancePercent,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CurrencyPairCode = DomainRules.Text(currencyPairCode, nameof(currencyPairCode));
        MaximumVariancePercent = DomainRules.Percent(maximumVariancePercent, nameof(maximumVariancePercent));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public string CurrencyPairCode { get; }
    public decimal MaximumVariancePercent { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
