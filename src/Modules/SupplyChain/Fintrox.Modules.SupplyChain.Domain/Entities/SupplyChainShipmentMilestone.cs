namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyChainShipmentMilestone domain record. No cross-module navigation properties.</summary>
public sealed class SupplyChainShipmentMilestone : OrganizationEntity
{
    public SupplyChainShipmentMilestone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid transportLegId,
        string milestoneCode,
        DateTimeOffset reachedAtUtc,
        string locationCode)
        : base(id, organizationId, createdAtUtc)
    {
        TransportLegId = DomainRules.Id(transportLegId, nameof(transportLegId));
        MilestoneCode = DomainRules.Text(milestoneCode, nameof(milestoneCode));
        ReachedAtUtc = DomainRules.Instant(reachedAtUtc, nameof(reachedAtUtc));
        LocationCode = DomainRules.Text(locationCode, nameof(locationCode));
    }

    public Guid TransportLegId { get; }
    public string MilestoneCode { get; }
    public DateTimeOffset ReachedAtUtc { get; }
    public string LocationCode { get; }
}
