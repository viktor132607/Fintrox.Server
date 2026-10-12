namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeAddress domain model.</summary>
public sealed class EmployeeAddress : OrganizationEntity
{
    public EmployeeAddress(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string addressType,
        string street,
        string city,
        string postalCode,
        string countryCode)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        AddressType = DomainRules.Text(addressType, nameof(addressType));
        Street = DomainRules.Text(street, nameof(street));
        City = DomainRules.Text(city, nameof(city));
        PostalCode = DomainRules.Text(postalCode, nameof(postalCode));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
    }

    public Guid EmployeeId { get; }
    public string AddressType { get; }
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string CountryCode { get; }
}
