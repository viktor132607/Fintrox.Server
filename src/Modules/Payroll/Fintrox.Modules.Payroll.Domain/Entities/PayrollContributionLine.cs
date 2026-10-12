namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollContributionLine; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollContributionLine : OrganizationEntity
{
    public PayrollContributionLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid payslipId,
        string contributionCode,
        decimal employeeAmount,
        decimal employerAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PayslipId = DomainRules.Id(payslipId, nameof(payslipId));
        ContributionCode = DomainRules.Text(contributionCode, nameof(contributionCode));
        EmployeeAmount = DomainRules.Nonnegative(employeeAmount, nameof(employeeAmount));
        EmployerAmount = DomainRules.Nonnegative(employerAmount, nameof(employerAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PayslipId { get; }
    public string ContributionCode { get; }
    public decimal EmployeeAmount { get; }
    public decimal EmployerAmount { get; }
    public string Currency { get; }
}
