namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentRefundRequest; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentRefundRequest : OrganizationEntity
{
    public PaymentRefundRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        Guid requestedByUserId,
        decimal amount,
        string currency,
        string reason,
        DateTimeOffset requestedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
    }

    public Guid PaymentId { get; }
    public Guid RequestedByUserId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
    public DateTimeOffset RequestedAtUtc { get; }
}
