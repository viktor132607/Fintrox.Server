namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceCalendar model; no cross-module navigations.</summary>
public sealed class ResourceCalendar : OrganizationEntity
{
    public ResourceCalendar(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        string calendarCode,
        string timeZoneId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        CalendarCode = DomainRules.Text(calendarCode, nameof(calendarCode));
        TimeZoneId = DomainRules.Text(timeZoneId, nameof(timeZoneId));
        IsActive = isActive;
    }

    public Guid ResourceId { get; }
    public string CalendarCode { get; }
    public string TimeZoneId { get; }
    public bool IsActive { get; }
}
