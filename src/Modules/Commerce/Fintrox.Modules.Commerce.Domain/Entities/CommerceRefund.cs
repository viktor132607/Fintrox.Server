namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CommerceRefund; external references validated by application layer.</summary>
public sealed class CommerceRefund : OrganizationEntity
{
    public CommerceRefund(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid commerceOrderId,
        Guid paymentId,
        decimal refundAmount,
        string currency,
        DateTimeOffset requestedAtUtc,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        CommerceOrderId = DomainRules.Id(commerceOrderId, nameof(commerceOrderId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        RefundAmount = DomainRules.Positive(refundAmount, nameof(refundAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid CommerceOrderId { get; }
    public Guid PaymentId { get; }
    public decimal RefundAmount { get; }
    public string Currency { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Reason { get; }
}
