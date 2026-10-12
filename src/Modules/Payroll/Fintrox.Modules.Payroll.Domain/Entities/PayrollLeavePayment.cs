namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollLeavePayment; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollLeavePayment : OrganizationEntity
{
    public PayrollLeavePayment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid leaveRequestId,
        Guid payPeriodId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        LeaveRequestId = DomainRules.Id(leaveRequestId, nameof(leaveRequestId));
        PayPeriodId = DomainRules.Id(payPeriodId, nameof(payPeriodId));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid EmployeeId { get; }
    public Guid LeaveRequestId { get; }
    public Guid PayPeriodId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
