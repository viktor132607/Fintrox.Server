namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeePositionAssignment domain model.</summary>
public sealed class EmployeePositionAssignment : OrganizationEntity
{
    public EmployeePositionAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid positionId,
        DateOnly startsOn,
        DateOnly endsOn,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        PositionId = DomainRules.Id(positionId, nameof(positionId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IsPrimary = isPrimary;
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public Guid PositionId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public bool IsPrimary { get; }
}
