namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetInventoryCount domain model.</summary>
public sealed class AssetInventoryCount : OrganizationEntity
{
    public AssetInventoryCount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid locationId,
        DateOnly countedOn,
        Guid countedByUserId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        LocationId = DomainRules.Id(locationId, nameof(locationId));
        CountedOn = DomainRules.Date(countedOn, nameof(countedOn));
        CountedByUserId = DomainRules.Id(countedByUserId, nameof(countedByUserId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid LocationId { get; }
    public DateOnly CountedOn { get; }
    public Guid CountedByUserId { get; }
    public string Status { get; }
}
