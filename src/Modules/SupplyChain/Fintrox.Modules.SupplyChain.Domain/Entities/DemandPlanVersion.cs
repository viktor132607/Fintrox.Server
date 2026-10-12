namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped DemandPlanVersion domain record. No cross-module navigation properties.</summary>
public sealed class DemandPlanVersion : OrganizationEntity
{
    public DemandPlanVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid demandForecastId,
        int versionNumber,
        DateOnly preparedOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        DemandForecastId = DomainRules.Id(demandForecastId, nameof(demandForecastId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        PreparedOn = DomainRules.Date(preparedOn, nameof(preparedOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid DemandForecastId { get; }
    public int VersionNumber { get; }
    public DateOnly PreparedOn { get; }
    public string Status { get; }
}
