namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionScrapRecord record.</summary>
public sealed class ProductionScrapRecord : OrganizationEntity
{
    public ProductionScrapRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid inventoryItemId,
        decimal quantity,
        string reason,
        DateOnly recordedOn)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        Reason = DomainRules.Text(reason, nameof(reason));
        RecordedOn = DomainRules.Date(recordedOn, nameof(recordedOn));
    }

    public Guid WorkOrderId { get; }
    public Guid InventoryItemId { get; }
    public decimal Quantity { get; }
    public string Reason { get; }
    public DateOnly RecordedOn { get; }
}
