namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollPayGroup; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollPayGroup : OrganizationEntity
{
    public PayrollPayGroup(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string paymentScheduleCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        PaymentScheduleCode = DomainRules.Text(paymentScheduleCode, nameof(paymentScheduleCode));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string PaymentScheduleCode { get; }
    public bool IsActive { get; }
}
