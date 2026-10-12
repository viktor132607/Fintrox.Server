namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollContributionRule; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollContributionRule : OrganizationEntity
{
    public PayrollContributionRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string jurisdictionCode,
        string contributionCode,
        decimal employeeRate,
        decimal employerRate,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionCode = DomainRules.Text(jurisdictionCode, nameof(jurisdictionCode));
        ContributionCode = DomainRules.Text(contributionCode, nameof(contributionCode));
        EmployeeRate = DomainRules.Percent(employeeRate, nameof(employeeRate));
        EmployerRate = DomainRules.Percent(employerRate, nameof(employerRate));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public string JurisdictionCode { get; }
    public string ContributionCode { get; }
    public decimal EmployeeRate { get; }
    public decimal EmployerRate { get; }
    public DateOnly EffectiveFrom { get; }
}
