namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationLocale record.</summary>
public sealed class OrganizationLocale : OrganizationEntity
{
    public OrganizationLocale(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string languageCode,
        string timeZoneId,
        string countryCode,
        string dateFormat)
        : base(id, organizationId, createdAtUtc)
    {
        LanguageCode = DomainRules.Text(languageCode, nameof(languageCode));
        TimeZoneId = DomainRules.Text(timeZoneId, nameof(timeZoneId));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        DateFormat = DomainRules.Text(dateFormat, nameof(dateFormat));
    }

    public string LanguageCode { get; }
    public string TimeZoneId { get; }
    public string CountryCode { get; }
    public string DateFormat { get; }
}
