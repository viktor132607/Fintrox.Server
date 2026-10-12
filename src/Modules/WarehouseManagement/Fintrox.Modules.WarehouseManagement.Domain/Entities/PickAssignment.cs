namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PickAssignment; tenant scope and input fields validated at creation.</summary>
public sealed class PickAssignment : OrganizationEntity
{
    public PickAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid pickingTaskId,
        Guid pickerUserId,
        DateTimeOffset assignedAtUtc,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        PickingTaskId = DomainRules.Id(pickingTaskId, nameof(pickingTaskId));
        PickerUserId = DomainRules.Id(pickerUserId, nameof(pickerUserId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid PickingTaskId { get; }
    public Guid PickerUserId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
    public int Priority { get; }
}
