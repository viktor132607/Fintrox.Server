namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionPaymentAttempt domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionPaymentAttempt : OrganizationEntity
{
    public SubscriptionPaymentAttempt(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid invoiceId,
        Guid paymentId,
        DateTimeOffset attemptAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        PaymentId = DomainRules.Id(paymentId, nameof(paymentId));
        AttemptAtUtc = DomainRules.Instant(attemptAtUtc, nameof(attemptAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SubscriptionId { get; }
    public Guid InvoiceId { get; }
    public Guid PaymentId { get; }
    public DateTimeOffset AttemptAtUtc { get; }
    public string Status { get; }
}
