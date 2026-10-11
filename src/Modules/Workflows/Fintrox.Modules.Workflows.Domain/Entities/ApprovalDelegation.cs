namespace Fintrox.Modules.Workflows.Domain.Entities;

public sealed class ApprovalDelegation : OrganizationEntity
{
    public ApprovalDelegation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fromUserId,
        Guid toUserId,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        FromUserId = DomainRules.Id(fromUserId, nameof(fromUserId));
        ToUserId = DomainRules.Id(toUserId, nameof(toUserId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
        if (FromUserId == ToUserId) throw new ArgumentException("Delegation requires another user.", nameof(toUserId));
    }

    public Guid FromUserId { get; }
    public Guid ToUserId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
