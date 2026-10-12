namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionCostAllocation record.</summary>
public sealed class ProductionCostAllocation : OrganizationEntity
{
    public ProductionCostAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid costPoolId,
        decimal allocatedAmount,
        string currency,
        string allocationBasis)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        CostPoolId = DomainRules.Id(costPoolId, nameof(costPoolId));
        AllocatedAmount = DomainRules.Nonnegative(allocatedAmount, nameof(allocatedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AllocationBasis = DomainRules.Text(allocationBasis, nameof(allocationBasis));
    }

    public Guid WorkOrderId { get; }
    public Guid CostPoolId { get; }
    public decimal AllocatedAmount { get; }
    public string Currency { get; }
    public string AllocationBasis { get; }
}
