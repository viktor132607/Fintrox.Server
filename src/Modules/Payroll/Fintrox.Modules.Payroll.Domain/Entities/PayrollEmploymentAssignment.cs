namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollEmploymentAssignment; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollEmploymentAssignment : OrganizationEntity
{
    public PayrollEmploymentAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid costCenterId,
        DateOnly startsOn,
        DateOnly endsOn,
        string payGroupCode)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        CostCenterId = DomainRules.Id(costCenterId, nameof(costCenterId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        PayGroupCode = DomainRules.Text(payGroupCode, nameof(payGroupCode));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid EmployeeId { get; }
    public Guid CostCenterId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string PayGroupCode { get; }
}
