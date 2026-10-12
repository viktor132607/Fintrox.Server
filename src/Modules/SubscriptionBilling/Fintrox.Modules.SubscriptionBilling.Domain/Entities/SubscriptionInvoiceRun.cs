namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionInvoiceRun domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionInvoiceRun : OrganizationEntity
{
    public SubscriptionInvoiceRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid billingCycleId,
        DateTimeOffset runAtUtc,
        int processedSubscriptions,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BillingCycleId = DomainRules.Id(billingCycleId, nameof(billingCycleId));
        RunAtUtc = DomainRules.Instant(runAtUtc, nameof(runAtUtc));
        ProcessedSubscriptions = DomainRules.NonnegativeInt(processedSubscriptions, nameof(processedSubscriptions));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid BillingCycleId { get; }
    public DateTimeOffset RunAtUtc { get; }
    public int ProcessedSubscriptions { get; }
    public string Status { get; }
}
