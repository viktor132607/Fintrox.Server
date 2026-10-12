namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowInstanceState; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowInstanceState : OrganizationEntity
{
    public WorkflowInstanceState(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowInstanceId,
        string stateCode,
        DateTimeOffset enteredAtUtc,
        bool isTerminal)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowInstanceId = DomainRules.Id(workflowInstanceId, nameof(workflowInstanceId));
        StateCode = DomainRules.Text(stateCode, nameof(stateCode));
        EnteredAtUtc = DomainRules.Instant(enteredAtUtc, nameof(enteredAtUtc));
        IsTerminal = isTerminal;
    }

    public Guid WorkflowInstanceId { get; }
    public string StateCode { get; }
    public DateTimeOffset EnteredAtUtc { get; }
    public bool IsTerminal { get; }
}
