namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseDock; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseDock : OrganizationEntity
{
    public WarehouseDock(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string dockCode,
        string dockType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        DockCode = DomainRules.Text(dockCode, nameof(dockCode));
        DockType = DomainRules.Text(dockType, nameof(dockType));
        IsActive = isActive;
    }

    public Guid WarehouseId { get; }
    public string DockCode { get; }
    public string DockType { get; }
    public bool IsActive { get; }
}
