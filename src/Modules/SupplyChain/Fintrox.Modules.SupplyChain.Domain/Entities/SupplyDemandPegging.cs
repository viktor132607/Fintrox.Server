namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyDemandPegging domain record. No cross-module navigation properties.</summary>
public sealed class SupplyDemandPegging : OrganizationEntity
{
    public SupplyDemandPegging(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid demandPlanLineId,
        Guid supplyPlanLineId,
        decimal allocatedQuantity,
        DateOnly allocationOn)
        : base(id, organizationId, createdAtUtc)
    {
        DemandPlanLineId = DomainRules.Id(demandPlanLineId, nameof(demandPlanLineId));
        SupplyPlanLineId = DomainRules.Id(supplyPlanLineId, nameof(supplyPlanLineId));
        AllocatedQuantity = DomainRules.Positive(allocatedQuantity, nameof(allocatedQuantity));
        AllocationOn = DomainRules.Date(allocationOn, nameof(allocationOn));
    }

    public Guid DemandPlanLineId { get; }
    public Guid SupplyPlanLineId { get; }
    public decimal AllocatedQuantity { get; }
    public DateOnly AllocationOn { get; }
}
