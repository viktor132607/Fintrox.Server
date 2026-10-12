namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ManufacturingTraceabilityLink record.</summary>
public sealed class ManufacturingTraceabilityLink : OrganizationEntity
{
    public ManufacturingTraceabilityLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid sourceLotId,
        Guid producedLotId,
        decimal linkedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        SourceLotId = DomainRules.Id(sourceLotId, nameof(sourceLotId));
        ProducedLotId = DomainRules.Id(producedLotId, nameof(producedLotId));
        LinkedQuantity = DomainRules.Positive(linkedQuantity, nameof(linkedQuantity));
    }

    public Guid WorkOrderId { get; }
    public Guid SourceLotId { get; }
    public Guid ProducedLotId { get; }
    public decimal LinkedQuantity { get; }
}
