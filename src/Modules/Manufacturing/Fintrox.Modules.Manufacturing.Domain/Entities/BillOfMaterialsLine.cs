namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class BillOfMaterialsLine : OrganizationEntity
{
    public BillOfMaterialsLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid billOfMaterialsId,
        Guid componentItemId,
        decimal quantity)
        : base(id, organizationId, createdAtUtc)
    {
        BillOfMaterialsId = DomainRules.Id(billOfMaterialsId, nameof(billOfMaterialsId));
        ComponentItemId = DomainRules.Id(componentItemId, nameof(componentItemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
    }

    public Guid BillOfMaterialsId { get; }
    public Guid ComponentItemId { get; }
    public decimal Quantity { get; }
}
