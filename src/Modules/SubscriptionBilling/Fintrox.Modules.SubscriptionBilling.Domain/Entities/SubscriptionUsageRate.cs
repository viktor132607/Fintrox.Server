namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionUsageRate domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionUsageRate : OrganizationEntity
{
    public SubscriptionUsageRate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid usageMeterId,
        decimal minimumUnits,
        decimal pricePerUnit,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        UsageMeterId = DomainRules.Id(usageMeterId, nameof(usageMeterId));
        MinimumUnits = DomainRules.Nonnegative(minimumUnits, nameof(minimumUnits));
        PricePerUnit = DomainRules.Nonnegative(pricePerUnit, nameof(pricePerUnit));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid UsageMeterId { get; }
    public decimal MinimumUnits { get; }
    public decimal PricePerUnit { get; }
    public string Currency { get; }
}
