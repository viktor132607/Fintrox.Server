namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeePersonalProfile domain model.</summary>
public sealed class EmployeePersonalProfile : OrganizationEntity
{
    public EmployeePersonalProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string preferredName,
        DateOnly birthDate,
        string nationalityCode,
        bool isVerified)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        PreferredName = DomainRules.Text(preferredName, nameof(preferredName));
        BirthDate = DomainRules.Date(birthDate, nameof(birthDate));
        NationalityCode = DomainRules.Country(nationalityCode, nameof(nationalityCode));
        IsVerified = isVerified;
    }

    public Guid EmployeeId { get; }
    public string PreferredName { get; }
    public DateOnly BirthDate { get; }
    public string NationalityCode { get; }
    public bool IsVerified { get; }
}
