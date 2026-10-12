namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueContractCurrencyRate model; no cross-module navigations.</summary>
public sealed class RevenueContractCurrencyRate : OrganizationEntity
{
    public RevenueContractCurrencyRate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revenueContractId,
        DateOnly conversionOn,
        string sourceCurrency,
        string targetCurrency,
        decimal appliedRate)
        : base(id, organizationId, createdAtUtc)
    {
        RevenueContractId = DomainRules.Id(revenueContractId, nameof(revenueContractId));
        ConversionOn = DomainRules.Date(conversionOn, nameof(conversionOn));
        SourceCurrency = DomainRules.Currency(sourceCurrency, nameof(sourceCurrency));
        TargetCurrency = DomainRules.Currency(targetCurrency, nameof(targetCurrency));
        AppliedRate = DomainRules.Positive(appliedRate, nameof(appliedRate));
    }

    public Guid RevenueContractId { get; }
    public DateOnly ConversionOn { get; }
    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public decimal AppliedRate { get; }
}
