namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeTermination domain model.</summary>
public sealed class EmployeeTermination : OrganizationEntity
{
    public EmployeeTermination(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateOnly terminationOn,
        string reason,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        TerminationOn = DomainRules.Date(terminationOn, nameof(terminationOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid EmployeeId { get; }
    public DateOnly TerminationOn { get; }
    public string Reason { get; }
    public Guid ApprovedByUserId { get; }
}
