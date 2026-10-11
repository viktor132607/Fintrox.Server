namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

public sealed class CycleCount : OrganizationEntity
{
    public CycleCount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid binLocationId,
        DateOnly scheduledOn)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        ScheduledOn = DomainRules.Date(scheduledOn, nameof(scheduledOn));
    }

    public Guid WarehouseId { get; }
    public Guid BinLocationId { get; }
    public DateOnly ScheduledOn { get; }
}
