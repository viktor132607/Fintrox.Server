namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetLocationHistory domain model.</summary>
public sealed class AssetLocationHistory : OrganizationEntity
{
    public AssetLocationHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        Guid assetLocationId,
        DateOnly movedOn,
        Guid movedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        AssetLocationId = DomainRules.Id(assetLocationId, nameof(assetLocationId));
        MovedOn = DomainRules.Date(movedOn, nameof(movedOn));
        MovedByUserId = DomainRules.Id(movedByUserId, nameof(movedByUserId));
    }

    public Guid FixedAssetId { get; }
    public Guid AssetLocationId { get; }
    public DateOnly MovedOn { get; }
    public Guid MovedByUserId { get; }
}
