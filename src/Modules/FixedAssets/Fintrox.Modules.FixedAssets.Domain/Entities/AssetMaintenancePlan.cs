namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetMaintenancePlan domain model.</summary>
public sealed class AssetMaintenancePlan : OrganizationEntity
{
    public AssetMaintenancePlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        string planCode,
        DateOnly nextServiceOn,
        int intervalDays)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        PlanCode = DomainRules.Text(planCode, nameof(planCode));
        NextServiceOn = DomainRules.Date(nextServiceOn, nameof(nextServiceOn));
        IntervalDays = DomainRules.PositiveInt(intervalDays, nameof(intervalDays));
    }

    public Guid FixedAssetId { get; }
    public string PlanCode { get; }
    public DateOnly NextServiceOn { get; }
    public int IntervalDays { get; }
}
