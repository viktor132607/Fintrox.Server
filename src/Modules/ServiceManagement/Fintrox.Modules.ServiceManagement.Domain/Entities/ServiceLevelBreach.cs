namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceLevelBreach domain record. No cross-module navigation properties.</summary>
public sealed class ServiceLevelBreach : OrganizationEntity
{
    public ServiceLevelBreach(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid serviceLevelMetricId,
        DateTimeOffset breachedAtUtc,
        int breachMinutes)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        ServiceLevelMetricId = DomainRules.Id(serviceLevelMetricId, nameof(serviceLevelMetricId));
        BreachedAtUtc = DomainRules.Instant(breachedAtUtc, nameof(breachedAtUtc));
        BreachMinutes = DomainRules.PositiveInt(breachMinutes, nameof(breachMinutes));
    }

    public Guid SupportTicketId { get; }
    public Guid ServiceLevelMetricId { get; }
    public DateTimeOffset BreachedAtUtc { get; }
    public int BreachMinutes { get; }
}
