namespace Fintrox.Modules.Payments.Domain.Entities;

public sealed class PaymentRefund : OrganizationEntity
{
    public PaymentRefund(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid originalPaymentId,
        string reference,
        decimal amount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        OriginalPaymentId = DomainRules.Id(originalPaymentId, nameof(originalPaymentId));
        Reference = DomainRules.Text(reference, nameof(reference));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid OriginalPaymentId { get; }
    public string Reference { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
