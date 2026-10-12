namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned PerformanceGoal domain model.</summary>
public sealed class PerformanceGoal : OrganizationEntity
{
    public PerformanceGoal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string goalName,
        decimal targetValue,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        GoalName = DomainRules.Text(goalName, nameof(goalName));
        TargetValue = DomainRules.Nonnegative(targetValue, nameof(targetValue));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid EmployeeId { get; }
    public string GoalName { get; }
    public decimal TargetValue { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
