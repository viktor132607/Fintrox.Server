namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PutawayConfirmation; tenant scope and input fields validated at creation.</summary>
public sealed class PutawayConfirmation : OrganizationEntity
{
    public PutawayConfirmation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid putawayTaskId,
        Guid inventoryItemId,
        decimal confirmedQuantity,
        DateTimeOffset confirmedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PutawayTaskId = DomainRules.Id(putawayTaskId, nameof(putawayTaskId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        ConfirmedQuantity = DomainRules.Positive(confirmedQuantity, nameof(confirmedQuantity));
        ConfirmedAtUtc = DomainRules.Instant(confirmedAtUtc, nameof(confirmedAtUtc));
    }

    public Guid PutawayTaskId { get; }
    public Guid InventoryItemId { get; }
    public decimal ConfirmedQuantity { get; }
    public DateTimeOffset ConfirmedAtUtc { get; }
}
