namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxJurisdictionRegistration; tenant scope and input fields validated at creation.</summary>
public sealed class TaxJurisdictionRegistration : OrganizationEntity
{
    public TaxJurisdictionRegistration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jurisdictionId,
        string registrationNumber,
        DateOnly registeredOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        RegistrationNumber = DomainRules.Text(registrationNumber, nameof(registrationNumber));
        RegisteredOn = DomainRules.Date(registeredOn, nameof(registeredOn));
        IsActive = isActive;
    }

    public Guid JurisdictionId { get; }
    public string RegistrationNumber { get; }
    public DateOnly RegisteredOn { get; }
    public bool IsActive { get; }
}
