namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseOrderChange model. References require application validation.</summary>
public sealed class PurchaseOrderChange : OrganizationEntity
{
    public PurchaseOrderChange(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderId,
        int revisionNumber,
        string reason,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        RevisionNumber = DomainRules.PositiveInt(revisionNumber, nameof(revisionNumber));
        Reason = DomainRules.Text(reason, nameof(reason));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
    }

    public Guid PurchaseOrderId { get; }
    public int RevisionNumber { get; }
    public string Reason { get; }
    public Guid ChangedByUserId { get; }
    public DateTimeOffset ChangedAtUtc { get; }
}
