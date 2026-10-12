namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxRuleVersion; tenant scope and input fields validated at creation.</summary>
public sealed class TaxRuleVersion : OrganizationEntity
{
    public TaxRuleVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jurisdictionId,
        string ruleCode,
        int versionNumber,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        RuleCode = DomainRules.Text(ruleCode, nameof(ruleCode));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid JurisdictionId { get; }
    public string RuleCode { get; }
    public int VersionNumber { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
