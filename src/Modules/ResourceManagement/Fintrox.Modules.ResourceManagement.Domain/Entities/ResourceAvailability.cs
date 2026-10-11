namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class ResourceAvailability : OrganizationEntity
{
    public ResourceAvailability(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        if (EndsAtUtc < StartsAtUtc) throw new ArgumentException("The end cannot precede the start.", nameof(endsAtUtc));
    }

    public Guid ResourceId { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
}
