namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionInvoiceItem domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionInvoiceItem : OrganizationEntity
{
    public SubscriptionInvoiceItem(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid invoiceRunId,
        Guid subscriptionId,
        string billingCode,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        InvoiceRunId = DomainRules.Id(invoiceRunId, nameof(invoiceRunId));
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        BillingCode = DomainRules.Text(billingCode, nameof(billingCode));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid InvoiceRunId { get; }
    public Guid SubscriptionId { get; }
    public string BillingCode { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
