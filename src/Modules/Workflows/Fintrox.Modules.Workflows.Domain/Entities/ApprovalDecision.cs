namespace Fintrox.Modules.Workflows.Domain.Entities;

public sealed class ApprovalDecision : OrganizationEntity
{
    public ApprovalDecision(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        Guid approvalStepId,
        Guid decidedByUserId,
        bool approved,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        ApprovalStepId = DomainRules.Id(approvalStepId, nameof(approvalStepId));
        DecidedByUserId = DomainRules.Id(decidedByUserId, nameof(decidedByUserId));
        Approved = approved;
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid WorkflowInstanceId { get; }
    public Guid ApprovalStepId { get; }
    public Guid DecidedByUserId { get; }
    public bool Approved { get; }
    public string Reason { get; }
}
