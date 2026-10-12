namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned CommerceOrderPayment; external references validated by application layer.</summary>
public sealed class CommerceOrderPayment : OrganizationEntity
{
    public CommerceOrderPayment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid commerceOrderId,
        Guid paymentId,
        decimal amount,
        string currency,
        string paymentStatus)
        : base(id, organizationId, createdAtUtc)
    {
        CommerceOrderId = DomainRules.Id(commerceOrderId, nameof(commerceOrderId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        PaymentStatus = DomainRules.Text(paymentStatus, nameof(paymentStatus));
    }

    public Guid CommerceOrderId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string PaymentStatus { get; }
}
