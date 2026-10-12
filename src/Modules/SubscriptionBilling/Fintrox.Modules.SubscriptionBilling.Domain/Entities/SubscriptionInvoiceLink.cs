namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionInvoiceLink domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionInvoiceLink : OrganizationEntity
{
    public SubscriptionInvoiceLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid invoiceId,
        Guid billingCycleId,
        DateTimeOffset linkedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        BillingCycleId = DomainRules.Id(billingCycleId, nameof(billingCycleId));
        LinkedAtUtc = DomainRules.Instant(linkedAtUtc, nameof(linkedAtUtc));
    }

    public Guid SubscriptionId { get; }
    public Guid InvoiceId { get; }
    public Guid BillingCycleId { get; }
    public DateTimeOffset LinkedAtUtc { get; }
}
