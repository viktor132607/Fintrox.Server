namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ShoppingCartLine; external references validated by application layer.</summary>
public sealed class ShoppingCartLine : OrganizationEntity
{
    public ShoppingCartLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cartId,
        Guid productVariantId,
        decimal quantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CartId = DomainRules.Id(cartId, nameof(cartId));
        ProductVariantId = DomainRules.Id(productVariantId, nameof(productVariantId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CartId { get; }
    public Guid ProductVariantId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
