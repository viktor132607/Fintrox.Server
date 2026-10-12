namespace Fintrox.Modules.Payroll.Domain.Entities;

/// <summary>Immutable tenant-scoped PayrollTaxRule; cross-module IDs are verified by use cases.</summary>
public sealed class PayrollTaxRule : OrganizationEntity
{
    public PayrollTaxRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string jurisdictionCode,
        string ruleCode,
        DateOnly validFrom,
        string ruleJson,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionCode = DomainRules.Text(jurisdictionCode, nameof(jurisdictionCode));
        RuleCode = DomainRules.Text(ruleCode, nameof(ruleCode));
        ValidFrom = DomainRules.Date(validFrom, nameof(validFrom));
        RuleJson = DomainRules.Json(ruleJson, nameof(ruleJson));
        IsActive = isActive;
    }

    public string JurisdictionCode { get; }
    public string RuleCode { get; }
    public DateOnly ValidFrom { get; }
    public string RuleJson { get; }
    public bool IsActive { get; }
}
