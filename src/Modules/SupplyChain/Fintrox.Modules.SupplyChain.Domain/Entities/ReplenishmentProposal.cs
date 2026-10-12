namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped ReplenishmentProposal domain record. No cross-module navigation properties.</summary>
public sealed class ReplenishmentProposal : OrganizationEntity
{
    public ReplenishmentProposal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid sourceWarehouseId,
        Guid targetWarehouseId,
        decimal proposedQuantity,
        DateOnly neededOn)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        SourceWarehouseId = DomainRules.Id(sourceWarehouseId, nameof(sourceWarehouseId));
        TargetWarehouseId = DomainRules.Id(targetWarehouseId, nameof(targetWarehouseId));
        ProposedQuantity = DomainRules.Positive(proposedQuantity, nameof(proposedQuantity));
        NeededOn = DomainRules.Date(neededOn, nameof(neededOn));
    }

    public Guid InventoryItemId { get; }
    public Guid SourceWarehouseId { get; }
    public Guid TargetWarehouseId { get; }
    public decimal ProposedQuantity { get; }
    public DateOnly NeededOn { get; }
}
