namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationComplianceRegistration record.</summary>
public sealed class OrganizationComplianceRegistration : OrganizationEntity
{
    public OrganizationComplianceRegistration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string registrationType,
        string registrationNumber,
        string countryCode,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        RegistrationType = DomainRules.Text(registrationType, nameof(registrationType));
        RegistrationNumber = DomainRules.Text(registrationNumber, nameof(registrationNumber));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public string RegistrationType { get; }
    public string RegistrationNumber { get; }
    public string CountryCode { get; }
    public DateOnly EffectiveFrom { get; }
}
