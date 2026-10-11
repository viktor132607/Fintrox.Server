namespace Fintrox.Modules.Workflows.Domain.Entities;

public sealed class WorkflowInstance : OrganizationEntity
{
    public WorkflowInstance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workflowDefinitionId,
        string resourceType,
        Guid resourceId,
        Guid requestedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        WorkflowDefinitionId = DomainRules.Id(workflowDefinitionId, nameof(workflowDefinitionId));
        ResourceType = DomainRules.Text(resourceType, nameof(resourceType));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
    }

    public Guid WorkflowDefinitionId { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }
    public Guid RequestedByUserId { get; }
}
