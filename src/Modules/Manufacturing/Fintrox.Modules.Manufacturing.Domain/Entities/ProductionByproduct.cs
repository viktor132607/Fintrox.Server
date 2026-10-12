namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionByproduct record.</summary>
public sealed class ProductionByproduct : OrganizationEntity
{
    public ProductionByproduct(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryItemId,
        decimal producedQuantity,
        DateOnly recordedOn)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ProducedQuantity = DomainRules.Positive(producedQuantity, nameof(producedQuantity));
        RecordedOn = DomainRules.Date(recordedOn, nameof(recordedOn));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal ProducedQuantity { get; }
    public DateOnly RecordedOn { get; }
}
