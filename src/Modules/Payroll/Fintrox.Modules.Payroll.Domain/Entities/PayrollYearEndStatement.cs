namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollYearEndStatement; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollYearEndStatement : OrganizationEntity
{
    public PayrollYearEndStatement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        int taxYear,
        decimal grossEarnings,
        decimal taxWithheld,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        TaxYear = DomainRules.PositiveInt(taxYear, nameof(taxYear));
        GrossEarnings = DomainRules.Nonnegative(grossEarnings, nameof(grossEarnings));
        TaxWithheld = DomainRules.Nonnegative(taxWithheld, nameof(taxWithheld));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid EmployeeId { get; }
    public int TaxYear { get; }
    public decimal GrossEarnings { get; }
    public decimal TaxWithheld { get; }
    public string Currency { get; }
}
