namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetDepreciationAdjustment domain model.</summary>
public sealed class AssetDepreciationAdjustment : OrganizationEntity
{
    public AssetDepreciationAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        Guid periodId,
        decimal adjustmentAmount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        PeriodId = DomainRules.Id(periodId, nameof(periodId));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid FixedAssetId { get; }
    public Guid PeriodId { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
