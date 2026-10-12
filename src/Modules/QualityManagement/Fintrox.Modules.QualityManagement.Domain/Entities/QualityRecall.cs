namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityRecall model. References require application validation.</summary>
public sealed class QualityRecall : OrganizationEntity
{
    public QualityRecall(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        string recallCode,
        DateOnly initiatedOn,
        string reason,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        RecallCode = DomainRules.Text(recallCode, nameof(recallCode));
        InitiatedOn = DomainRules.Date(initiatedOn, nameof(initiatedOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid InventoryItemId { get; }
    public string RecallCode { get; }
    public DateOnly InitiatedOn { get; }
    public string Reason { get; }
    public string Status { get; }
}
