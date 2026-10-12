namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationExchangeRateSet; external references validated by application layer.</summary>
public sealed class ConsolidationExchangeRateSet : OrganizationEntity
{
    public ConsolidationExchangeRateSet(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        Guid periodId,
        string providerCode,
        DateOnly effectiveOn,
        bool isLocked)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        PeriodId = DomainRules.Id(periodId, nameof(periodId));
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        EffectiveOn = DomainRules.Date(effectiveOn, nameof(effectiveOn));
        IsLocked = isLocked;
    }

    public Guid CompanyGroupId { get; }
    public Guid PeriodId { get; }
    public string ProviderCode { get; }
    public DateOnly EffectiveOn { get; }
    public bool IsLocked { get; }
}
