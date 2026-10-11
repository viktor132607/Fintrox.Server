namespace Fintrox.Modules.Counterparties.Domain.Entities;

public sealed class CounterpartyAddress : OrganizationEntity
{
    public CounterpartyAddress(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string addressLine,
        string city,
        string postalCode,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        AddressLine = DomainRules.Text(addressLine, nameof(addressLine));
        City = DomainRules.Text(city, nameof(city));
        PostalCode = DomainRules.Text(postalCode, nameof(postalCode));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public Guid CounterpartyId { get; }
    public string AddressLine { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string CountryCode { get; }
}
