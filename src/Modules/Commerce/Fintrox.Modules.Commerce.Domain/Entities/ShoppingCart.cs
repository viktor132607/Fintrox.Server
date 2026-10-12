namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned ShoppingCart; external references validated by application layer.</summary>
public sealed class ShoppingCart : OrganizationEntity
{
    public ShoppingCart(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid customerId,
        Guid storefrontId,
        string currency,
        string status,
        DateTimeOffset expiresAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CustomerId = DomainRules.Id(customerId, nameof(customerId));
        StorefrontId = DomainRules.Id(storefrontId, nameof(storefrontId));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
    }

    public Guid CustomerId { get; }
    public Guid StorefrontId { get; }
    public string Currency { get; }
    public string Status { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
}
