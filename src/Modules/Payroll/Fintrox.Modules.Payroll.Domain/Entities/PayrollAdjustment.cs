namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollAdjustment; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollAdjustment : OrganizationEntity
{
    public PayrollAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid periodId,
        string adjustmentType,
        decimal amount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        PeriodId = DomainRules.Id(periodId, nameof(periodId));
        AdjustmentType = DomainRules.Text(adjustmentType, nameof(adjustmentType));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid EmployeeId { get; }
    public Guid PeriodId { get; }
    public string AdjustmentType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
