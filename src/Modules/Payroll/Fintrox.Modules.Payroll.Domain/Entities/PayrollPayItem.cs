namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollPayItem; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollPayItem : OrganizationEntity
{
    public PayrollPayItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid employeeId,
        string componentType,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        ComponentType = DomainRules.Text(componentType, nameof(componentType));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PayRunId { get; }
    public Guid EmployeeId { get; }
    public string ComponentType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
