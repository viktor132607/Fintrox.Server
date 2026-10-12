namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowAutomationError; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowAutomationError : OrganizationEntity
{
    public WorkflowAutomationError(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid automationRunId,
        string errorCode,
        string errorMessage,
        DateTimeOffset occurredAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        AutomationRunId = DomainRules.Id(automationRunId, nameof(automationRunId));
        ErrorCode = DomainRules.Text(errorCode, nameof(errorCode));
        ErrorMessage = DomainRules.Text(errorMessage, nameof(errorMessage));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
    }

    public Guid AutomationRunId { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
    public DateTimeOffset OccurredAtUtc { get; }
}
