namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionCompletion record.</summary>
public sealed class ProductionCompletion : OrganizationEntity
{
    public ProductionCompletion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        DateOnly completedOn,
        decimal completedQuantity,
        Guid recordedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        CompletedOn = DomainRules.Date(completedOn, nameof(completedOn));
        CompletedQuantity = DomainRules.Nonnegative(completedQuantity, nameof(completedQuantity));
        RecordedByUserId = DomainRules.Id(recordedByUserId, nameof(recordedByUserId));
    }

    public Guid WorkOrderId { get; }
    public DateOnly CompletedOn { get; }
    public decimal CompletedQuantity { get; }
    public Guid RecordedByUserId { get; }
}
