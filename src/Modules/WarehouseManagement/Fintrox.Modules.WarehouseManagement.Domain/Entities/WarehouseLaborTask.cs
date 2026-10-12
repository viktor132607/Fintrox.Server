namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseLaborTask; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseLaborTask : OrganizationEntity
{
    public WarehouseLaborTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid assignedUserId,
        string taskType,
        DateTimeOffset dueAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        AssignedUserId = DomainRules.Id(assignedUserId, nameof(assignedUserId));
        TaskType = DomainRules.Text(taskType, nameof(taskType));
        DueAtUtc = DomainRules.Instant(dueAtUtc, nameof(dueAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public Guid AssignedUserId { get; }
    public string TaskType { get; }
    public DateTimeOffset DueAtUtc { get; }
    public string Status { get; }
}
