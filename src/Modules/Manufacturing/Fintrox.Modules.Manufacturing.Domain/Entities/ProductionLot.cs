namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionLot record.</summary>
public sealed class ProductionLot : OrganizationEntity
{
    public ProductionLot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        string lotNumber,
        DateOnly producedOn,
        decimal goodQuantity,
        decimal rejectedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        LotNumber = DomainRules.Text(lotNumber, nameof(lotNumber));
        ProducedOn = DomainRules.Date(producedOn, nameof(producedOn));
        GoodQuantity = DomainRules.Nonnegative(goodQuantity, nameof(goodQuantity));
        RejectedQuantity = DomainRules.Nonnegative(rejectedQuantity, nameof(rejectedQuantity));
    }

    public Guid WorkOrderId { get; }
    public string LotNumber { get; }
    public DateOnly ProducedOn { get; }
    public decimal GoodQuantity { get; }
    public decimal RejectedQuantity { get; }
}
