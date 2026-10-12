namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseZone; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseZone : OrganizationEntity
{
    public WarehouseZone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string zoneCode,
        string zoneType,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ZoneCode = DomainRules.Text(zoneCode, nameof(zoneCode));
        ZoneType = DomainRules.Text(zoneType, nameof(zoneType));
        IsActive = isActive;
    }

    public Guid WarehouseId { get; }
    public string ZoneCode { get; }
    public string ZoneType { get; }
    public bool IsActive { get; }
}
