namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceLeaveBlock model; no cross-module navigations.</summary>
public sealed class ResourceLeaveBlock : OrganizationEntity
{
    public ResourceLeaveBlock(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        DateOnly startsOn,
        DateOnly endsOn,
        string leaveType)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        LeaveType = DomainRules.Text(leaveType, nameof(leaveType));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid ResourceId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string LeaveType { get; }
}
