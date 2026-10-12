namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PromotionRule; external references validated by application layer.</summary>
public sealed class PromotionRule : OrganizationEntity
{
    public PromotionRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid promotionId,
        string ruleType,
        string conditionJson,
        decimal discountPercent,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PromotionId = DomainRules.Id(promotionId, nameof(promotionId));
        RuleType = DomainRules.Text(ruleType, nameof(ruleType));
        ConditionJson = DomainRules.Json(conditionJson, nameof(conditionJson));
        DiscountPercent = DomainRules.Percent(discountPercent, nameof(discountPercent));
        IsActive = isActive;
    }

    public Guid PromotionId { get; }
    public string RuleType { get; }
    public string ConditionJson { get; }
    public decimal DiscountPercent { get; }
    public bool IsActive { get; }
}
