namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PackingStation; tenant scope and input fields validated at creation.</summary>
public sealed class PackingStation : OrganizationEntity
{
    public PackingStation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string stationCode,
        string name,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        StationCode = DomainRules.Text(stationCode, nameof(stationCode));
        Name = DomainRules.Text(name, nameof(name));
        IsActive = isActive;
    }

    public Guid WarehouseId { get; }
    public string StationCode { get; }
    public string Name { get; }
    public bool IsActive { get; }
}
