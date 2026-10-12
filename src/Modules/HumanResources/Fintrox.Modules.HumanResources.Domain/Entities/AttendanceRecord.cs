namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned AttendanceRecord domain model.</summary>
public sealed class AttendanceRecord : OrganizationEntity
{
    public AttendanceRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateTimeOffset clockInAtUtc,
        DateTimeOffset clockOutAtUtc,
        string source)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        ClockInAtUtc = DomainRules.Instant(clockInAtUtc, nameof(clockInAtUtc));
        ClockOutAtUtc = DomainRules.Instant(clockOutAtUtc, nameof(clockOutAtUtc));
        Source = DomainRules.Text(source, nameof(source));
    }

    public Guid EmployeeId { get; }
    public DateTimeOffset ClockInAtUtc { get; }
    public DateTimeOffset ClockOutAtUtc { get; }
    public string Source { get; }
}
