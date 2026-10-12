namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped PostingRuleVersion domain record; referenced objects are validated by application use cases.</summary>
public sealed class PostingRuleVersion : OrganizationEntity
{
    public PostingRuleVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string ruleCode,
        int versionNumber,
        DateOnly effectiveFrom,
        DateOnly effectiveTo,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        RuleCode = DomainRules.Text(ruleCode, nameof(ruleCode));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        EffectiveTo = DomainRules.Date(effectiveTo, nameof(effectiveTo));
        IsEnabled = isEnabled;
        if (EffectiveTo < EffectiveFrom) throw new ArgumentException("End must not precede start.", nameof(effectiveTo));
    }

    public string RuleCode { get; }
    public int VersionNumber { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly EffectiveTo { get; }
    public bool IsEnabled { get; }
}
