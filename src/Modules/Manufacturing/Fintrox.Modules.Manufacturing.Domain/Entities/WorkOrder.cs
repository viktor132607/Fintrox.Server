namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class WorkOrder : OrganizationEntity
{
    public WorkOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid billOfMaterialsId,
        Guid routingId,
        decimal quantity,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        BillOfMaterialsId = DomainRules.Id(billOfMaterialsId, nameof(billOfMaterialsId));
        RoutingId = DomainRules.Id(routingId, nameof(routingId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid BillOfMaterialsId { get; }
    public Guid RoutingId { get; }
    public decimal Quantity { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
