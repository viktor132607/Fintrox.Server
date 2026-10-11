namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

public sealed class Timesheet : OrganizationEntity
{
    public Timesheet(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public Guid ResourceId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
