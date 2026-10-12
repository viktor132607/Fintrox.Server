namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRateOverride. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRateOverride : OrganizationEntity
{
    public CurrencyRateOverride(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string sourceCurrency,
        string targetCurrency,
        decimal rate,
        DateOnly effectiveOn,
        string reason,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        SourceCurrency = DomainRules.Currency(sourceCurrency, nameof(sourceCurrency));
        TargetCurrency = DomainRules.Currency(targetCurrency, nameof(targetCurrency));
        Rate = DomainRules.Positive(rate, nameof(rate));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public decimal Rate { get; }
    public DateOnly EffectiveOn { get; }
    public string Reason { get; }
    public Guid ApprovedByUserId { get; }
}
