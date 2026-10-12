namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetInventoryCountLine domain model.</summary>
public sealed class AssetInventoryCountLine : OrganizationEntity
{
    public AssetInventoryCountLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid countId,
        Guid fixedAssetId,
        bool isFound,
        string condition,
        string notes)
        : base(id, organizationId, createdAtUtc)
    {
        CountId = DomainRules.Id(countId, nameof(countId));
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        IsFound = isFound;
        Condition = DomainRules.Text(condition, nameof(condition));
        Notes = DomainRules.Text(notes, nameof(notes));
    }

    public Guid CountId { get; }
    public Guid FixedAssetId { get; }
    public bool IsFound { get; }
    public string Condition { get; }
    public string Notes { get; }
}
