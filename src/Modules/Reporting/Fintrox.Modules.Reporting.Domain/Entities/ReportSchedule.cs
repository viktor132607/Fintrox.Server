namespace Fintrox.Modules.Reporting.Domain.Entities;

public sealed class ReportSchedule : OrganizationEntity
{
    public ReportSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        Guid requestedByUserId,
        DateTimeOffset nextRunAtUtc,
        int intervalMinutes)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        NextRunAtUtc = DomainRules.Instant(nextRunAtUtc, nameof(nextRunAtUtc));
        IntervalMinutes = DomainRules.PositiveInt(intervalMinutes, nameof(intervalMinutes));
    }

    public Guid ReportDefinitionId { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset NextRunAtUtc { get; }
    public int IntervalMinutes { get; }
}
