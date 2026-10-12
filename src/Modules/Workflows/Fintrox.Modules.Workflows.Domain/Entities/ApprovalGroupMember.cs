namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalGroupMember; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalGroupMember : OrganizationEntity
{
    public ApprovalGroupMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid approvalGroupId,
        Guid userId,
        DateOnly joinedOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ApprovalGroupId = DomainRules.Id(approvalGroupId, nameof(approvalGroupId));
        UserId = DomainRules.Id(userId, nameof(userId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
        IsActive = isActive;
    }

    public Guid ApprovalGroupId { get; }
    public Guid UserId { get; }
    public DateOnly JoinedOn { get; }
    public bool IsActive { get; }
}
