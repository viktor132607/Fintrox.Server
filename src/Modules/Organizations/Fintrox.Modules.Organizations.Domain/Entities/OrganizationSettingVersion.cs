namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationSettingVersion record.</summary>
public sealed class OrganizationSettingVersion : OrganizationEntity
{
    public OrganizationSettingVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string settingCode,
        int versionNumber,
        string valueJson,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        SettingCode = DomainRules.Text(settingCode, nameof(settingCode));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        ValueJson = DomainRules.Json(valueJson, nameof(valueJson));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public string SettingCode { get; }
    public int VersionNumber { get; }
    public string ValueJson { get; }
    public DateOnly EffectiveFrom { get; }
}
