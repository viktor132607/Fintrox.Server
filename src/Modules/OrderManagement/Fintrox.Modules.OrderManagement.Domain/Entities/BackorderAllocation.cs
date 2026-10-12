namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned BackorderAllocation record.</summary>
public sealed class BackorderAllocation : OrganizationEntity
{
    public BackorderAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid backorderId,
        Guid inventoryItemId,
        decimal reservedQuantity,
        DateOnly estimatedFulfillmentOn)
        : base(id, organizationId, createdAtUtc)
    {
        BackorderId = DomainRules.Id(backorderId, nameof(backorderId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ReservedQuantity = DomainRules.Positive(reservedQuantity, nameof(reservedQuantity));
        EstimatedFulfillmentOn = DomainRules.Date(estimatedFulfillmentOn, nameof(estimatedFulfillmentOn));
    }

    public Guid BackorderId { get; }
    public Guid InventoryItemId { get; }
    public decimal ReservedQuantity { get; }
    public DateOnly EstimatedFulfillmentOn { get; }
}
