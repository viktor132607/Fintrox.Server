namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned LeaveAccrual domain model.</summary>
public sealed class LeaveAccrual : OrganizationEntity
{
    public LeaveAccrual(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string leaveType,
        DateOnly accruedOn,
        decimal accruedDays,
        string periodCode)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        LeaveType = DomainRules.Text(leaveType, nameof(leaveType));
        AccruedOn = DomainRules.Date(accruedOn, nameof(accruedOn));
        AccruedDays = DomainRules.Nonnegative(accruedDays, nameof(accruedDays));
        PeriodCode = DomainRules.Text(periodCode, nameof(periodCode));
    }

    public Guid EmployeeId { get; }
    public string LeaveType { get; }
    public DateOnly AccruedOn { get; }
    public decimal AccruedDays { get; }
    public string PeriodCode { get; }
}
