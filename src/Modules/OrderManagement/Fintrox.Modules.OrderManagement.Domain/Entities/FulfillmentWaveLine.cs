namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned FulfillmentWaveLine record.</summary>
public sealed class FulfillmentWaveLine : OrganizationEntity
{
    public FulfillmentWaveLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid waveId,
        Guid fulfillmentOrderId,
        Guid fulfillmentLineId,
        decimal allocatedQuantity)
        : base(id, organizationId, createdAtUtc)
    {
        WaveId = DomainRules.Id(waveId, nameof(waveId));
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        FulfillmentLineId = DomainRules.Id(fulfillmentLineId, nameof(fulfillmentLineId));
        AllocatedQuantity = DomainRules.Positive(allocatedQuantity, nameof(allocatedQuantity));
    }

    public Guid WaveId { get; }
    public Guid FulfillmentOrderId { get; }
    public Guid FulfillmentLineId { get; }
    public decimal AllocatedQuantity { get; }
}
