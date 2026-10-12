namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalEscalation; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalEscalation : OrganizationEntity
{
    public ApprovalEscalation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid approvalStepId,
        Guid escalateToUserId,
        DateTimeOffset escalationAtUtc,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        ApprovalStepId = DomainRules.Id(approvalStepId, nameof(approvalStepId));
        EscalateToUserId = DomainRules.Id(escalateToUserId, nameof(escalateToUserId));
        EscalationAtUtc = DomainRules.Instant(escalationAtUtc, nameof(escalationAtUtc));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid ApprovalStepId { get; }
    public Guid EscalateToUserId { get; }
    public DateTimeOffset EscalationAtUtc { get; }
    public string Reason { get; }
}
