namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollOvertimeRecord; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollOvertimeRecord : OrganizationEntity
{
    public PayrollOvertimeRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        DateOnly workDate,
        decimal hours,
        decimal multiplier,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        WorkDate = DomainRules.Date(workDate, nameof(workDate));
        Hours = DomainRules.Positive(hours, nameof(hours));
        Multiplier = DomainRules.Positive(multiplier, nameof(multiplier));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid EmployeeId { get; }
    public DateOnly WorkDate { get; }
    public decimal Hours { get; }
    public decimal Multiplier { get; }
    public Guid ApprovedByUserId { get; }
}
