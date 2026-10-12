namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned LeaveBalance domain model.</summary>
public sealed class LeaveBalance : OrganizationEntity
{
    public LeaveBalance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string leaveType,
        int year,
        decimal entitlementDays,
        decimal remainingDays)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        LeaveType = DomainRules.Text(leaveType, nameof(leaveType));
        Year = DomainRules.PositiveInt(year, nameof(year));
        EntitlementDays = DomainRules.Nonnegative(entitlementDays, nameof(entitlementDays));
        RemainingDays = DomainRules.Nonnegative(remainingDays, nameof(remainingDays));
    }

    public Guid EmployeeId { get; }
    public string LeaveType { get; }
    public int Year { get; }
    public decimal EntitlementDays { get; }
    public decimal RemainingDays { get; }
}
