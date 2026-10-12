namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionMaterialIssue record.</summary>
public sealed class ProductionMaterialIssue : OrganizationEntity
{
    public ProductionMaterialIssue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryMovementId,
        Guid inventoryItemId,
        decimal issuedQuantity,
        DateOnly issuedOn)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryMovementId = DomainRules.Id(inventoryMovementId, nameof(inventoryMovementId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        IssuedQuantity = DomainRules.Positive(issuedQuantity, nameof(issuedQuantity));
        IssuedOn = DomainRules.Date(issuedOn, nameof(issuedOn));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryMovementId { get; }
    public Guid InventoryItemId { get; }
    public decimal IssuedQuantity { get; }
    public DateOnly IssuedOn { get; }
}
