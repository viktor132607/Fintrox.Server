namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseRequestApproval model. References require application validation.</summary>
public sealed class PurchaseRequestApproval : OrganizationEntity
{
    public PurchaseRequestApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseRequisitionId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseRequisitionId = DomainRules.Id(purchaseRequisitionId, nameof(purchaseRequisitionId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid PurchaseRequisitionId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
