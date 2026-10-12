namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PriceTier; external references validated by application layer.</summary>
public sealed class PriceTier : OrganizationEntity
{
    public PriceTier(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid priceListId,
        decimal minimumQuantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PriceListId = DomainRules.Id(priceListId, nameof(priceListId));
        MinimumQuantity = DomainRules.Positive(minimumQuantity, nameof(minimumQuantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PriceListId { get; }
    public decimal MinimumQuantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
