namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationRule; external references validated by application layer.</summary>
public sealed class ConsolidationRule : OrganizationEntity
{
    public ConsolidationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid companyGroupId,
        string ruleCode,
        string ruleType,
        int priority,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CompanyGroupId = DomainRules.Id(companyGroupId, nameof(companyGroupId));
        RuleCode = DomainRules.Text(ruleCode, nameof(ruleCode));
        RuleType = DomainRules.Text(ruleType, nameof(ruleType));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
        IsActive = isActive;
    }

    public Guid CompanyGroupId { get; }
    public string RuleCode { get; }
    public string RuleType { get; }
    public int Priority { get; }
    public bool IsActive { get; }
}
