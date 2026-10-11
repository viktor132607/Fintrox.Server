namespace Fintrox.Modules.Workflows.Domain.Entities;

public sealed class ApprovalStep : OrganizationEntity
{
    public ApprovalStep(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowDefinitionId,
        int sequence,
        Guid approverRoleId)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowDefinitionId = DomainRules.Id(workflowDefinitionId, nameof(workflowDefinitionId));
        Sequence = DomainRules.PositiveInt(sequence, nameof(sequence));
        ApproverRoleId = DomainRules.Id(approverRoleId, nameof(approverRoleId));
    }

    public Guid WorkflowDefinitionId { get; }
    public int Sequence { get; }
    public Guid ApproverRoleId { get; }
}
