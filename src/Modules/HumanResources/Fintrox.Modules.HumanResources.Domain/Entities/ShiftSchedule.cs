namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned ShiftSchedule domain model.</summary>
public sealed class ShiftSchedule : OrganizationEntity
{
    public ShiftSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string locationCode,
        string shiftType)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        LocationCode = DomainRules.Text(locationCode, nameof(locationCode));
        ShiftType = DomainRules.Text(shiftType, nameof(shiftType));
    }

    public Guid EmployeeId { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
    public string LocationCode { get; }
    public string ShiftType { get; }
}
