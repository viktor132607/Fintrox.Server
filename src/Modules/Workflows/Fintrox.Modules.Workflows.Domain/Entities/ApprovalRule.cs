namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned ApprovalRule; tenant scope and input fields validated at creation.</summary>
public sealed class ApprovalRule : OrganizationEntity
{
    public ApprovalRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid approvalPolicyId,
        string conditionJson,
        string approverRole,
        int sequenceNumber)
        : base(id, organizationId, createdAtUtc)
    {
        ApprovalPolicyId = DomainRules.Id(approvalPolicyId, nameof(approvalPolicyId));
        ConditionJson = DomainRules.Json(conditionJson, nameof(conditionJson));
        ApproverRole = DomainRules.Text(approverRole, nameof(approverRole));
        SequenceNumber = DomainRules.PositiveInt(sequenceNumber, nameof(sequenceNumber));
    }

    public Guid ApprovalPolicyId { get; }
    public string ConditionJson { get; }
    public string ApproverRole { get; }
    public int SequenceNumber { get; }
}
