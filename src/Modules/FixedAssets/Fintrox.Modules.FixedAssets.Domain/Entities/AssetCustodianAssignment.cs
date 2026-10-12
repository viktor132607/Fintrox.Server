namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetCustodianAssignment domain model.</summary>
public sealed class AssetCustodianAssignment : OrganizationEntity
{
    public AssetCustodianAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        Guid custodianUserId,
        DateOnly assignedOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        CustodianUserId = DomainRules.Id(custodianUserId, nameof(custodianUserId));
        AssignedOn = DomainRules.Date(assignedOn, nameof(assignedOn));
        IsActive = isActive;
    }

    public Guid FixedAssetId { get; }
    public Guid CustodianUserId { get; }
    public DateOnly AssignedOn { get; }
    public bool IsActive { get; }
}
