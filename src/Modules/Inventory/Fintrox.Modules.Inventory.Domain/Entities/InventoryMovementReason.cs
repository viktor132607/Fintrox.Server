namespace Fintrox.Modules.Inventory.Domain.Entities;

/// <summary>Immutable tenant-owned InventoryMovementReason domain record; references checked in application layer.</summary>
public sealed class InventoryMovementReason : OrganizationEntity
{
    public InventoryMovementReason(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        bool isAdjustment,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        IsAdjustment = isAdjustment;
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public bool IsAdjustment { get; }
    public bool IsActive { get; }
}
