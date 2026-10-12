namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCollectionActivity model; no cross-module navigations.</summary>
public sealed class SalesCollectionActivity : OrganizationEntity
{
    public SalesCollectionActivity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid collectionCaseId,
        string channel,
        DateOnly activityDate,
        string outcome,
        Guid ownerUserId)
        : base(id, organizationId, createdAtUtc)
    {
        CollectionCaseId = DomainRules.Id(collectionCaseId, nameof(collectionCaseId));
        Channel = DomainRules.Text(channel, nameof(channel));
        ActivityDate = DomainRules.Date(activityDate, nameof(activityDate));
        Outcome = DomainRules.Text(outcome, nameof(outcome));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
    }

    public Guid CollectionCaseId { get; }
    public string Channel { get; }
    public DateOnly ActivityDate { get; }
    public string Outcome { get; }
    public Guid OwnerUserId { get; }
}
