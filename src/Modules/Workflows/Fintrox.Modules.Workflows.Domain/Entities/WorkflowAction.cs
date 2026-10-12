namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowAction; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowAction : OrganizationEntity
{
    public WorkflowAction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowVersionId,
        string actionType,
        string targetCode,
        string configurationJson)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowVersionId = DomainRules.Id(workflowVersionId, nameof(workflowVersionId));
        ActionType = DomainRules.Text(actionType, nameof(actionType));
        TargetCode = DomainRules.Text(targetCode, nameof(targetCode));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
    }

    public Guid WorkflowVersionId { get; }
    public string ActionType { get; }
    public string TargetCode { get; }
    public string ConfigurationJson { get; }
}
