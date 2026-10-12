namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollLocalizationConfig; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollLocalizationConfig : OrganizationEntity
{
    public PayrollLocalizationConfig(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string countryCode,
        string jurisdictionCode,
        string policyVersion,
        DateOnly effectiveFrom,
        string configurationJson)
        : base(id, organizationId, createdAtUtc)
    {
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        JurisdictionCode = DomainRules.Text(jurisdictionCode, nameof(jurisdictionCode));
        PolicyVersion = DomainRules.Text(policyVersion, nameof(policyVersion));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
    }

    public string CountryCode { get; }
    public string JurisdictionCode { get; }
    public string PolicyVersion { get; }
    public DateOnly EffectiveFrom { get; }
    public string ConfigurationJson { get; }
}
