namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceBookingConflict model; no cross-module navigations.</summary>
public sealed class ResourceBookingConflict : OrganizationEntity
{
    public ResourceBookingConflict(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceBookingId,
        Guid conflictingBookingId,
        DateTimeOffset detectedAtUtc,
        string severity)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceBookingId = DomainRules.Id(resourceBookingId, nameof(resourceBookingId));
        ConflictingBookingId = DomainRules.Id(conflictingBookingId, nameof(conflictingBookingId));
        DetectedAtUtc = DomainRules.Instant(detectedAtUtc, nameof(detectedAtUtc));
        Severity = DomainRules.Text(severity, nameof(severity));
    }

    public Guid ResourceBookingId { get; }
    public Guid ConflictingBookingId { get; }
    public DateTimeOffset DetectedAtUtc { get; }
    public string Severity { get; }
}
