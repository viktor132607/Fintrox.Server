namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class TimesheetEntry : OrganizationEntity
{
    public TimesheetEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid timesheetId,
        Guid projectId,
        DateOnly workDate,
        decimal hours)
        : base(id, organizationId, createdAtUtc)
    {
        TimesheetId = DomainRules.Id(timesheetId, nameof(timesheetId));
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        WorkDate = DomainRules.Date(workDate, nameof(workDate));
        Hours = DomainRules.Hours(hours, nameof(hours));
    }

    public Guid TimesheetId { get; }
    public Guid ProjectId { get; }
    public DateOnly WorkDate { get; }
    public decimal Hours { get; }
}
