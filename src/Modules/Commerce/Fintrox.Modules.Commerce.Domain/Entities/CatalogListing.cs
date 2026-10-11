namespace Fintrox.Modules.Commerce.Domain.Entities;

public sealed class CatalogListing : OrganizationEntity
{
    public CatalogListing(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid storefrontId,
        string externalSku)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        StorefrontId = DomainRules.Id(storefrontId, nameof(storefrontId));
        ExternalSku = DomainRules.Text(externalSku, nameof(externalSku));
    }

    public Guid ItemId { get; }
    public Guid StorefrontId { get; }
    public string ExternalSku { get; }
}
