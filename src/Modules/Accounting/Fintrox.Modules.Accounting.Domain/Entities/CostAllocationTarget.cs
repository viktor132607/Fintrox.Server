namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped CostAllocationTarget domain record; referenced objects are validated by application use cases.</summary>
public sealed class CostAllocationTarget : OrganizationEntity
{
    public CostAllocationTarget(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid allocationRuleId,
        Guid targetDimensionId,
        Guid targetDimensionValueId,
        decimal sharePercent)
        : base(id, organizationId, createdAtUtc)
    {
        AllocationRuleId = DomainRules.Id(allocationRuleId, nameof(allocationRuleId));
        TargetDimensionId = DomainRules.Id(targetDimensionId, nameof(targetDimensionId));
        TargetDimensionValueId = DomainRules.Id(targetDimensionValueId, nameof(targetDimensionValueId));
        SharePercent = DomainRules.Percent(sharePercent, nameof(sharePercent));
    }

    public Guid AllocationRuleId { get; }
    public Guid TargetDimensionId { get; }
    public Guid TargetDimensionValueId { get; }
    public decimal SharePercent { get; }
}
