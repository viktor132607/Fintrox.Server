namespace Fintrox.Modules.Payroll.Domain.Entities;

public sealed class PayRun : OrganizationEntity
{
    public PayRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payrollPeriodId,
        string reference,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayrollPeriodId = DomainRules.Id(payrollPeriodId, nameof(payrollPeriodId));
        Reference = DomainRules.Text(reference, nameof(reference));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PayrollPeriodId { get; }
    public string Reference { get; }
    public string Currency { get; }
}
