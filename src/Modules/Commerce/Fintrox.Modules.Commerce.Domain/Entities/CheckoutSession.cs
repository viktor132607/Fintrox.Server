namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CheckoutSession; external references validated by application layer.</summary>
public sealed class CheckoutSession : OrganizationEntity
{
    public CheckoutSession(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cartId,
        Guid customerId,
        DateTimeOffset startedAtUtc,
        DateTimeOffset expiresAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CartId = DomainRules.Id(cartId, nameof(cartId));
        CustomerId = DomainRules.Id(customerId, nameof(customerId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        ExpiresAtUtc = DomainRules.Instant(expiresAtUtc, nameof(expiresAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CartId { get; }
    public Guid CustomerId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public DateTimeOffset ExpiresAtUtc { get; }
    public string Status { get; }
}
