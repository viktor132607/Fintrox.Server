namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryExpiryRule domain record; references checked in application layer.</summary>
public sealed class InventoryExpiryRule : OrganizationEntity
{
    public InventoryExpiryRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        int daysBeforeExpiry,
        string disposalPolicy,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        DaysBeforeExpiry = DomainRules.NonnegativeInt(daysBeforeExpiry, nameof(daysBeforeExpiry));
        DisposalPolicy = DomainRules.Text(disposalPolicy, nameof(disposalPolicy));
        IsActive = isActive;
    }

    public Guid InventoryItemId { get; }
    public int DaysBeforeExpiry { get; }
    public string DisposalPolicy { get; }
    public bool IsActive { get; }
}
