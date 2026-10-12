namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned StorageCapacityProfile; tenant scope and input fields validated at creation.</summary>
public sealed class StorageCapacityProfile : OrganizationEntity
{
    public StorageCapacityProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid binLocationId,
        decimal maximumWeight,
        decimal maximumVolume,
        bool isHazardousAllowed)
        : base(id, organizationId, createdAtUtc)
    {
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        MaximumWeight = DomainRules.Nonnegative(maximumWeight, nameof(maximumWeight));
        MaximumVolume = DomainRules.Nonnegative(maximumVolume, nameof(maximumVolume));
        IsHazardousAllowed = isHazardousAllowed;
    }

    public Guid BinLocationId { get; }
    public decimal MaximumWeight { get; }
    public decimal MaximumVolume { get; }
    public bool IsHazardousAllowed { get; }
}
