namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowServiceLevel; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowServiceLevel : OrganizationEntity
{
    public WorkflowServiceLevel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        string slaCode,
        int targetMinutes,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        SlaCode = DomainRules.Text(slaCode, nameof(slaCode));
        TargetMinutes = DomainRules.PositiveInt(targetMinutes, nameof(targetMinutes));
        IsActive = isActive;
    }

    public Guid WorkflowVersionId { get; }
    public string SlaCode { get; }
    public int TargetMinutes { get; }
    public bool IsActive { get; }
}
