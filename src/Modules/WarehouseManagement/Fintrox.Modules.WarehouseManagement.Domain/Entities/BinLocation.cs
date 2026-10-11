namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class BinLocation : OrganizationEntity
{
    public BinLocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string code)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        Code = DomainRules.Text(code, nameof(code));
    }

    public Guid WarehouseId { get; }
    public string Code { get; }
}
