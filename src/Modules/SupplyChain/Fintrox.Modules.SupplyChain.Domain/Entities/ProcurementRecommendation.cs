namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped ProcurementRecommendation domain record. No cross-module navigation properties.</summary>
public sealed class ProcurementRecommendation : OrganizationEntity
{
    public ProcurementRecommendation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inventoryItemId,
        Guid supplierId,
        decimal recommendedQuantity,
        DateOnly requiredOn,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        RecommendedQuantity = DomainRules.Positive(recommendedQuantity, nameof(recommendedQuantity));
        RequiredOn = DomainRules.Date(requiredOn, nameof(requiredOn));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid InventoryItemId { get; }
    public Guid SupplierId { get; }
    public decimal RecommendedQuantity { get; }
    public DateOnly RequiredOn { get; }
    public string Reason { get; }
}
