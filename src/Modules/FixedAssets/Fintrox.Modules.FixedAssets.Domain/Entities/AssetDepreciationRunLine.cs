namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetDepreciationRunLine domain model.</summary>
public sealed class AssetDepreciationRunLine : OrganizationEntity
{
    public AssetDepreciationRunLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid runId,
        Guid fixedAssetId,
        decimal depreciationAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RunId = DomainRules.Id(runId, nameof(runId));
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        DepreciationAmount = DomainRules.Nonnegative(depreciationAmount, nameof(depreciationAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RunId { get; }
    public Guid FixedAssetId { get; }
    public decimal DepreciationAmount { get; }
    public string Currency { get; }
}
