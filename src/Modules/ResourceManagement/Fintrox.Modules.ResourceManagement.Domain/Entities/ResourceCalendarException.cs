namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCalendarException model; no cross-module navigations.</summary>
public sealed class ResourceCalendarException : OrganizationEntity
{
    public ResourceCalendarException(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid calendarId,
        DateOnly exceptionDate,
        string exceptionType,
        decimal availableHours)
        : base(id, organizationId, createdAtUtc)
    {
        CalendarId = DomainRules.Id(calendarId, nameof(calendarId));
        ExceptionDate = DomainRules.Date(exceptionDate, nameof(exceptionDate));
        ExceptionType = DomainRules.Text(exceptionType, nameof(exceptionType));
        AvailableHours = DomainRules.Nonnegative(availableHours, nameof(availableHours));
    }

    public Guid CalendarId { get; }
    public DateOnly ExceptionDate { get; }
    public string ExceptionType { get; }
    public decimal AvailableHours { get; }
}
