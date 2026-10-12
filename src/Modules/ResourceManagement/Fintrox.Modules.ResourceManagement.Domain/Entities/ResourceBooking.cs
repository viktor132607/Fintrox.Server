namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceBooking model; no cross-module navigations.</summary>
public sealed class ResourceBooking : OrganizationEntity
{
    public ResourceBooking(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        Guid projectId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        decimal bookedHours)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        BookedHours = DomainRules.Positive(bookedHours, nameof(bookedHours));
    }

    public Guid ResourceId { get; }
    public Guid ProjectId { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
    public decimal BookedHours { get; }
}
