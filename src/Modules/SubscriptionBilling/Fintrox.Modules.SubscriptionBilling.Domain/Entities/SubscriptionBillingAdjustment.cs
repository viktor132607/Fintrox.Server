namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionBillingAdjustment domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionBillingAdjustment : OrganizationEntity
{
    public SubscriptionBillingAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid invoiceId,
        decimal adjustmentAmount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid SubscriptionId { get; }
    public Guid InvoiceId { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
