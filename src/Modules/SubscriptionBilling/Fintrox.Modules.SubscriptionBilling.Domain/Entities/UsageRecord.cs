namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

public sealed class UsageRecord : OrganizationEntity
{
    public UsageRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        string meterCode,
        decimal quantity,
        DateTimeOffset occurredAtUtc,
        string externalReference)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        MeterCode = DomainRules.Text(meterCode, nameof(meterCode));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        ExternalReference = DomainRules.Text(externalReference, nameof(externalReference));
    }

    public Guid SubscriptionId { get; }
    public string MeterCode { get; }
    public decimal Quantity { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string ExternalReference { get; }
}
