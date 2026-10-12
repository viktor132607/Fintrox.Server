namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowWebhookAction; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowWebhookAction : OrganizationEntity
{
    public WorkflowWebhookAction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowActionId,
        Guid integrationSubscriptionId,
        string eventCode,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowActionId = DomainRules.Id(workflowActionId, nameof(workflowActionId));
        IntegrationSubscriptionId = DomainRules.Id(integrationSubscriptionId, nameof(integrationSubscriptionId));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        IsEnabled = isEnabled;
    }

    public Guid WorkflowActionId { get; }
    public Guid IntegrationSubscriptionId { get; }
    public string EventCode { get; }
    public bool IsEnabled { get; }
}
