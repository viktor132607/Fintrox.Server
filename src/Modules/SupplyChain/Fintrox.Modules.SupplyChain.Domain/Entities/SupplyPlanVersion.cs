namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyPlanVersion domain record. No cross-module navigation properties.</summary>
public sealed class SupplyPlanVersion : OrganizationEntity
{
    public SupplyPlanVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplyPlanId,
        int versionNumber,
        DateOnly preparedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SupplyPlanId = DomainRules.Id(supplyPlanId, nameof(supplyPlanId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        PreparedOn = DomainRules.Date(preparedOn, nameof(preparedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SupplyPlanId { get; }
    public int VersionNumber { get; }
    public DateOnly PreparedOn { get; }
    public string Status { get; }
}
