namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollPaymentInstruction; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollPaymentInstruction : OrganizationEntity
{
    public PayrollPaymentInstruction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid employeeId,
        Guid bankAccountId,
        decimal netAmount,
        string currency,
        DateOnly valueDate)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        NetAmount = DomainRules.Nonnegative(netAmount, nameof(netAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ValueDate = DomainRules.Date(valueDate, nameof(valueDate));
    }

    public Guid PayRunId { get; }
    public Guid EmployeeId { get; }
    public Guid BankAccountId { get; }
    public decimal NetAmount { get; }
    public string Currency { get; }
    public DateOnly ValueDate { get; }
}
