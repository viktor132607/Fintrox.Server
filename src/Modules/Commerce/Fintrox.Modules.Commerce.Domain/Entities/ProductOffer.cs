namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ProductOffer; external references validated by application layer.</summary>
public sealed class ProductOffer : OrganizationEntity
{
    public ProductOffer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid catalogListingId,
        string offerCode,
        decimal salePrice,
        string currency,
        DateOnly availableFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CatalogListingId = DomainRules.Id(catalogListingId, nameof(catalogListingId));
        OfferCode = DomainRules.Text(offerCode, nameof(offerCode));
        SalePrice = DomainRules.Nonnegative(salePrice, nameof(salePrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AvailableFrom = DomainRules.Date(availableFrom, nameof(availableFrom));
        IsActive = isActive;
    }

    public Guid CatalogListingId { get; }
    public string OfferCode { get; }
    public decimal SalePrice { get; }
    public string Currency { get; }
    public DateOnly AvailableFrom { get; }
    public bool IsActive { get; }
}
