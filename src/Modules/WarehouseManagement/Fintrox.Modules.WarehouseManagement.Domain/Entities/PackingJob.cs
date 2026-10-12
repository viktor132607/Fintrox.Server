namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PackingJob; tenant scope and input fields validated at creation.</summary>
public sealed class PackingJob : OrganizationEntity
{
    public PackingJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid packingStationId,
        Guid shipmentId,
        Guid packedByUserId,
        DateTimeOffset packedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        PackingStationId = DomainRules.Id(packingStationId, nameof(packingStationId));
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        PackedByUserId = DomainRules.Id(packedByUserId, nameof(packedByUserId));
        PackedAtUtc = DomainRules.Instant(packedAtUtc, nameof(packedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid PackingStationId { get; }
    public Guid ShipmentId { get; }
    public Guid PackedByUserId { get; }
    public DateTimeOffset PackedAtUtc { get; }
    public string Status { get; }
}
