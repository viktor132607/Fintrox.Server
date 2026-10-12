namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetTransfer domain model.</summary>
public sealed class AssetTransfer : OrganizationEntity
{
    public AssetTransfer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        Guid fromLocationId,
        Guid toLocationId,
        DateOnly transferredOn,
        Guid authorizedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        FromLocationId = DomainRules.Id(fromLocationId, nameof(fromLocationId));
        ToLocationId = DomainRules.Id(toLocationId, nameof(toLocationId));
        TransferredOn = DomainRules.Date(transferredOn, nameof(transferredOn));
        AuthorizedByUserId = DomainRules.Id(authorizedByUserId, nameof(authorizedByUserId));
    }

    public Guid FixedAssetId { get; }
    public Guid FromLocationId { get; }
    public Guid ToLocationId { get; }
    public DateOnly TransferredOn { get; }
    public Guid AuthorizedByUserId { get; }
}
