namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned UserLocalePreference domain model.</summary>
public sealed class UserLocalePreference : OrganizationEntity
{
    public UserLocalePreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string languageCode,
        string timeZoneId,
        string dateFormat,
        string numberFormat)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        LanguageCode = DomainRules.Text(languageCode, nameof(languageCode));
        TimeZoneId = DomainRules.Text(timeZoneId, nameof(timeZoneId));
        DateFormat = DomainRules.Text(dateFormat, nameof(dateFormat));
        NumberFormat = DomainRules.Text(numberFormat, nameof(numberFormat));
    }

    public Guid UserId { get; }
    public string LanguageCode { get; }
    public string TimeZoneId { get; }
    public string DateFormat { get; }
    public string NumberFormat { get; }
}
