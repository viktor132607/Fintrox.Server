namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceLevelMeasurement domain record. No cross-module navigation properties.</summary>
public sealed class ServiceLevelMeasurement : OrganizationEntity
{
    public ServiceLevelMeasurement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid serviceLevelMetricId,
        int measuredMinutes,
        DateTimeOffset measuredAtUtc,
        bool isBreached)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        ServiceLevelMetricId = DomainRules.Id(serviceLevelMetricId, nameof(serviceLevelMetricId));
        MeasuredMinutes = DomainRules.NonnegativeInt(measuredMinutes, nameof(measuredMinutes));
        MeasuredAtUtc = DomainRules.Instant(measuredAtUtc, nameof(measuredAtUtc));
        IsBreached = isBreached;
    }

    public Guid SupportTicketId { get; }
    public Guid ServiceLevelMetricId { get; }
    public int MeasuredMinutes { get; }
    public DateTimeOffset MeasuredAtUtc { get; }
    public bool IsBreached { get; }
}
