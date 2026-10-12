namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionDunningStep domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionDunningStep : OrganizationEntity
{
    public SubscriptionDunningStep(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        Guid invoiceId,
        int attemptNumber,
        DateOnly dueOn,
        string actionCode)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        AttemptNumber = DomainRules.PositiveInt(attemptNumber, nameof(attemptNumber));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        ActionCode = DomainRules.Text(actionCode, nameof(actionCode));
    }

    public Guid SubscriptionId { get; }
    public Guid InvoiceId { get; }
    public int AttemptNumber { get; }
    public DateOnly DueOn { get; }
    public string ActionCode { get; }
}
