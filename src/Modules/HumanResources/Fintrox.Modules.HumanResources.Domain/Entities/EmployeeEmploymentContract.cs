namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeEmploymentContract domain model.</summary>
public sealed class EmployeeEmploymentContract : OrganizationEntity
{
    public EmployeeEmploymentContract(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string contractNumber,
        DateOnly startsOn,
        DateOnly endsOn,
        string employmentType)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        ContractNumber = DomainRules.Text(contractNumber, nameof(contractNumber));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        EmploymentType = DomainRules.Text(employmentType, nameof(employmentType));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public string ContractNumber { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string EmploymentType { get; }
}
