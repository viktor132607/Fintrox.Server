namespace Fintrox.Modules.Commerce.Domain.Entities;

public sealed class PriceListEntry : OrganizationEntity
{
    public PriceListEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid priceListId,
        Guid itemId,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PriceListId = DomainRules.Id(priceListId, nameof(priceListId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PriceListId { get; }
    public Guid ItemId { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
