namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCostRate model; no cross-module navigations.</summary>
public sealed class ResourceCostRate : OrganizationEntity
{
    public ResourceCostRate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        decimal hourlyCost,
        string currency,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        HourlyCost = DomainRules.Nonnegative(hourlyCost, nameof(hourlyCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid ResourceId { get; }
    public decimal HourlyCost { get; }
    public string Currency { get; }
    public DateOnly EffectiveFrom { get; }
}
