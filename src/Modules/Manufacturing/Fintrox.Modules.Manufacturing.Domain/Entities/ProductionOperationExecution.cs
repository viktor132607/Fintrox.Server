namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionOperationExecution record.</summary>
public sealed class ProductionOperationExecution : OrganizationEntity
{
    public ProductionOperationExecution(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid routingOperationId,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        RoutingOperationId = DomainRules.Id(routingOperationId, nameof(routingOperationId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        CompletedAtUtc = DomainRules.Instant(completedAtUtc, nameof(completedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WorkOrderId { get; }
    public Guid RoutingOperationId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public DateTimeOffset CompletedAtUtc { get; }
    public string Status { get; }
}
