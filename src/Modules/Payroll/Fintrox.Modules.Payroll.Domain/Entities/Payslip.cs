namespace Fintrox.Modules.Payroll.Domain.Entities;

public sealed class Payslip : OrganizationEntity
{
    public Payslip(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payRunId,
        Guid employeeId,
        decimal grossPay,
        decimal deductions,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayRunId = DomainRules.Id(payRunId, nameof(payRunId));
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        GrossPay = DomainRules.Nonnegative(grossPay, nameof(grossPay));
        Deductions = DomainRules.Nonnegative(deductions, nameof(deductions));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (Deductions > GrossPay) throw new ArgumentException("Deductions cannot exceed gross pay.", nameof(deductions));
    }

    public Guid PayRunId { get; }
    public Guid EmployeeId { get; }
    public decimal GrossPay { get; }
    public decimal Deductions { get; }
    public string Currency { get; }
    public decimal NetPay => GrossPay - Deductions;
}
