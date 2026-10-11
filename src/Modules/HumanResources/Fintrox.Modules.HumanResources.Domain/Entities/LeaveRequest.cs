namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class LeaveRequest : OrganizationEntity
{
    public LeaveRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateOnly startsOn,
        DateOnly endsOn,
        string leaveType)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        LeaveType = DomainRules.Text(leaveType, nameof(leaveType));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string LeaveType { get; }
}
