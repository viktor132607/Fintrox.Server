namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionSchedule record.</summary>
public sealed class ProductionSchedule : OrganizationEntity
{
    public ProductionSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workOrderId,
        Guid workCenterId,
        DateTimeOffset plannedStartUtc,
        DateTimeOffset plannedFinishUtc,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        WorkOrderId = DomainRules.Id(workOrderId, nameof(workOrderId));
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        PlannedStartUtc = DomainRules.Instant(plannedStartUtc, nameof(plannedStartUtc));
        PlannedFinishUtc = DomainRules.Instant(plannedFinishUtc, nameof(plannedFinishUtc));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid WorkOrderId { get; }
    public Guid WorkCenterId { get; }
    public DateTimeOffset PlannedStartUtc { get; }
    public DateTimeOffset PlannedFinishUtc { get; }
    public int Priority { get; }
}
