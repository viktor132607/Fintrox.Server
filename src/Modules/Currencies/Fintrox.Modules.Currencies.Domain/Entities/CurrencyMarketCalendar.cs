namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyMarketCalendar. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyMarketCalendar : OrganizationEntity
{
    public CurrencyMarketCalendar(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string currency,
        string countryCode,
        DateOnly holidayDate,
        string holidayName)
        : base(id, organizationId, createdAtUtc)
    {
        Currency = DomainRules.Currency(currency, nameof(currency));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        HolidayDate = DomainRules.Date(holidayDate, nameof(holidayDate));
        HolidayName = DomainRules.Text(holidayName, nameof(holidayName));
    }

    public string Currency { get; }
    public string CountryCode { get; }
    public DateOnly HolidayDate { get; }
    public string HolidayName { get; }
}
