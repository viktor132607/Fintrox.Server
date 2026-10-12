namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentFee; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentFee : OrganizationEntity
{
    public PaymentFee(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid paymentId,
        string feeType,
        decimal amount,
        string currency,
        DateOnly chargedOn)
        : base(id, organizationId, createdAtUtc)
    {
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        FeeType = DomainRules.Text(feeType, nameof(feeType));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ChargedOn = DomainRules.Date(chargedOn, nameof(chargedOn));
    }

    public Guid PaymentId { get; }
    public string FeeType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateOnly ChargedOn { get; }
}
