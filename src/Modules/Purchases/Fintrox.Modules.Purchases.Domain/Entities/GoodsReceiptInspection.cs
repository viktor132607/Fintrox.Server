namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped GoodsReceiptInspection model. References require application validation.</summary>
public sealed class GoodsReceiptInspection : OrganizationEntity
{
    public GoodsReceiptInspection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid goodsReceiptLineId,
        Guid inspectionPlanId,
        DateOnly inspectedOn,
        string result)
        : base(id, organizationId, createdAtUtc)
    {
        GoodsReceiptLineId = DomainRules.Id(goodsReceiptLineId, nameof(goodsReceiptLineId));
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        InspectedOn = DomainRules.Date(inspectedOn, nameof(inspectedOn));
        Result = DomainRules.Text(result, nameof(result));
    }

    public Guid GoodsReceiptLineId { get; }
    public Guid InspectionPlanId { get; }
    public DateOnly InspectedOn { get; }
    public string Result { get; }
}
