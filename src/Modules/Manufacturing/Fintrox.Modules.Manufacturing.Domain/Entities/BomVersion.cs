namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned BomVersion record.</summary>
public sealed class BomVersion : OrganizationEntity
{
    public BomVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid billOfMaterialsId,
        int versionNumber,
        DateOnly effectiveFrom,
        bool isReleased)
        : base(id, organizationId, createdAtUtc)
    {
        BillOfMaterialsId = DomainRules.Id(billOfMaterialsId, nameof(billOfMaterialsId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsReleased = isReleased;
    }

    public Guid BillOfMaterialsId { get; }
    public int VersionNumber { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsReleased { get; }
}
