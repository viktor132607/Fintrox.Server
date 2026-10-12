namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationLegalEntity record.</summary>
public sealed class OrganizationLegalEntity : OrganizationEntity
{
    public OrganizationLegalEntity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string companyCode,
        string registeredName,
        string registrationNumber,
        string countryCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyCode = DomainRules.Text(companyCode, nameof(companyCode));
        RegisteredName = DomainRules.Text(registeredName, nameof(registeredName));
        RegistrationNumber = DomainRules.Text(registrationNumber, nameof(registrationNumber));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        IsActive = isActive;
    }

    public string CompanyCode { get; }
    public string RegisteredName { get; }
    public string RegistrationNumber { get; }
    public string CountryCode { get; }
    public bool IsActive { get; }
}
