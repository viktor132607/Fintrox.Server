namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetDisposalLine domain model.</summary>
public sealed class AssetDisposalLine : OrganizationEntity
{
    public AssetDisposalLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid assetDisposalId,
        Guid fixedAssetId,
        decimal proceeds,
        decimal carryingValue,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        AssetDisposalId = DomainRules.Id(assetDisposalId, nameof(assetDisposalId));
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        Proceeds = DomainRules.Nonnegative(proceeds, nameof(proceeds));
        CarryingValue = DomainRules.Nonnegative(carryingValue, nameof(carryingValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid AssetDisposalId { get; }
    public Guid FixedAssetId { get; }
    public decimal Proceeds { get; }
    public decimal CarryingValue { get; }
    public string Currency { get; }
}
