namespace Fintrox.Modules.Organizations.Domain.Entities;

public sealed class TaxRegistration : OrganizationEntity
{
    public TaxRegistration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string countryCode,
        string registrationNumber,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        RegistrationNumber = DomainRules.Text(registrationNumber, nameof(registrationNumber));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public string CountryCode { get; }
    public string RegistrationNumber { get; }
    public DateOnly EffectiveFrom { get; }
}
