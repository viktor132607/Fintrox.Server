namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeTraining domain model.</summary>
public sealed class EmployeeTraining : OrganizationEntity
{
    public EmployeeTraining(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string courseCode,
        DateOnly startsOn,
        DateOnly endsOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        CourseCode = DomainRules.Text(courseCode, nameof(courseCode));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Status = DomainRules.Text(status, nameof(status));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public string CourseCode { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Status { get; }
}
