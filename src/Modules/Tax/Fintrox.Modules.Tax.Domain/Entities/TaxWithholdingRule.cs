namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxWithholdingRule; tenant scope and input fields validated at creation.</summary>
public sealed class TaxWithholdingRule : OrganizationEntity
{
    public TaxWithholdingRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jurisdictionId,
        string ruleCode,
        decimal withholdingPercent,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        RuleCode = DomainRules.Text(ruleCode, nameof(ruleCode));
        WithholdingPercent = DomainRules.Percent(withholdingPercent, nameof(withholdingPercent));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid JurisdictionId { get; }
    public string RuleCode { get; }
    public decimal WithholdingPercent { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
