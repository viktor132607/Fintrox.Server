namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityTraceabilityRecord model. References require application validation.</summary>
public sealed class QualityTraceabilityRecord : OrganizationEntity
{
    public QualityTraceabilityRecord(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryLotId,
        Guid sourceMovementId,
        Guid relatedLotId,
        DateTimeOffset recordedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryLotId = DomainRules.Id(inventoryLotId, nameof(inventoryLotId));
        SourceMovementId = DomainRules.Id(sourceMovementId, nameof(sourceMovementId));
        RelatedLotId = DomainRules.Id(relatedLotId, nameof(relatedLotId));
        RecordedAtUtc = DomainRules.Instant(recordedAtUtc, nameof(recordedAtUtc));
    }

    public Guid InventoryLotId { get; }
    public Guid SourceMovementId { get; }
    public Guid RelatedLotId { get; }
    public DateTimeOffset RecordedAtUtc { get; }
}
