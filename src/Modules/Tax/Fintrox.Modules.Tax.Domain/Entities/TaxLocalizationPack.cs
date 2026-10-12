namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxLocalizationPack; tenant scope and input fields validated at creation.</summary>
public sealed class TaxLocalizationPack : OrganizationEntity
{
    public TaxLocalizationPack(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string countryCode,
        string packVersion,
        DateOnly effectiveFrom,
        string configurationJson,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        PackVersion = DomainRules.Text(packVersion, nameof(packVersion));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
        IsActive = isActive;
    }

    public string CountryCode { get; }
    public string PackVersion { get; }
    public DateOnly EffectiveFrom { get; }
    public string ConfigurationJson { get; }
    public bool IsActive { get; }
}
