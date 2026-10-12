namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationOperatingLocation record.</summary>
public sealed class OrganizationOperatingLocation : OrganizationEntity
{
    public OrganizationOperatingLocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string locationCode,
        string name,
        string street,
        string city,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        LocationCode = DomainRules.Text(locationCode, nameof(locationCode));
        Name = DomainRules.Text(name, nameof(name));
        Street = DomainRules.Text(street, nameof(street));
        City = DomainRules.Text(city, nameof(city));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public string LocationCode { get; }
    public string Name { get; }
    public string Street { get; }
    public string City { get; }
    public string CountryCode { get; }
}
