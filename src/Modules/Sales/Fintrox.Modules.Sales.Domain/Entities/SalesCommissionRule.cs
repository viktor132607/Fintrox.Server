namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCommissionRule model; no cross-module navigations.</summary>
public sealed class SalesCommissionRule : OrganizationEntity
{
    public SalesCommissionRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesPersonId,
        decimal commissionPercent,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        SalesPersonId = DomainRules.Id(salesPersonId, nameof(salesPersonId));
        CommissionPercent = DomainRules.Percent(commissionPercent, nameof(commissionPercent));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid SalesPersonId { get; }
    public decimal CommissionPercent { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
