namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentWriteOffRequest; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentWriteOffRequest : OrganizationEntity
{
    public PaymentWriteOffRequest(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        decimal amount,
        string currency,
        string reason,
        Guid requestedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
    }

    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
    public Guid RequestedByUserId { get; }
}
