namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ProductVariant; external references validated by application layer.</summary>
public sealed class ProductVariant : OrganizationEntity
{
    public ProductVariant(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid catalogListingId,
        string sku,
        string barcode,
        string variantName,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CatalogListingId = DomainRules.Id(catalogListingId, nameof(catalogListingId));
        Sku = DomainRules.Text(sku, nameof(sku));
        Barcode = DomainRules.Text(barcode, nameof(barcode));
        VariantName = DomainRules.Text(variantName, nameof(variantName));
        IsActive = isActive;
    }

    public Guid CatalogListingId { get; }
    public string Sku { get; }
    public string Barcode { get; }
    public string VariantName { get; }
    public bool IsActive { get; }
}
