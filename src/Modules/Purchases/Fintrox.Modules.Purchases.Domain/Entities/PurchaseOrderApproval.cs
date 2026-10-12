namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseOrderApproval model. References require application validation.</summary>
public sealed class PurchaseOrderApproval : OrganizationEntity
{
    public PurchaseOrderApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseOrderId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseOrderId = DomainRules.Id(purchaseOrderId, nameof(purchaseOrderId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid PurchaseOrderId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
