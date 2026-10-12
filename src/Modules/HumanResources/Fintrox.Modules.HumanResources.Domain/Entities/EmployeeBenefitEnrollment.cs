namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeBenefitEnrollment domain model.</summary>
public sealed class EmployeeBenefitEnrollment : OrganizationEntity
{
    public EmployeeBenefitEnrollment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string benefitPlanCode,
        DateOnly startsOn,
        DateOnly endsOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        BenefitPlanCode = DomainRules.Text(benefitPlanCode, nameof(benefitPlanCode));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IsActive = isActive;
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public string BenefitPlanCode { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public bool IsActive { get; }
}
