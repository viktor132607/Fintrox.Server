namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationAddress record.</summary>
public sealed class OrganizationAddress : OrganizationEntity
{
    public OrganizationAddress(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid legalEntityId,
        string addressType,
        string street,
        string city,
        string postalCode,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        LegalEntityId = DomainRules.Id(legalEntityId, nameof(legalEntityId));
        AddressType = DomainRules.Text(addressType, nameof(addressType));
        Street = DomainRules.Text(street, nameof(street));
        City = DomainRules.Text(city, nameof(city));
        PostalCode = DomainRules.Text(postalCode, nameof(postalCode));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public Guid LegalEntityId { get; }
    public string AddressType { get; }
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string CountryCode { get; }
}
