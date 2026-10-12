namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseRequisitionAttachment model. References require application validation.</summary>
public sealed class PurchaseRequisitionAttachment : OrganizationEntity
{
    public PurchaseRequisitionAttachment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseRequisitionId,
        Guid documentId,
        DateTimeOffset uploadedAtUtc,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseRequisitionId = DomainRules.Id(purchaseRequisitionId, nameof(purchaseRequisitionId));
        DocumentId = DomainRules.Id(documentId, nameof(documentId));
        UploadedAtUtc = DomainRules.Instant(uploadedAtUtc, nameof(uploadedAtUtc));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid PurchaseRequisitionId { get; }
    public Guid DocumentId { get; }
    public DateTimeOffset UploadedAtUtc { get; }
    public string Description { get; }
}
