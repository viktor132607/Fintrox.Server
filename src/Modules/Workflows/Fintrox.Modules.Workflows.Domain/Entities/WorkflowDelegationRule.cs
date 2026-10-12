namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowDelegationRule; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowDelegationRule : OrganizationEntity
{
    public WorkflowDelegationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid delegatorUserId,
        Guid delegateUserId,
        DateOnly startsOn,
        DateOnly endsOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        DelegatorUserId = DomainRules.Id(delegatorUserId, nameof(delegatorUserId));
        DelegateUserId = DomainRules.Id(delegateUserId, nameof(delegateUserId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IsActive = isActive;
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid DelegatorUserId { get; }
    public Guid DelegateUserId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public bool IsActive { get; }
}
