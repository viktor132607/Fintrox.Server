namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PutawayAssignment; tenant scope and input fields validated at creation.</summary>
public sealed class PutawayAssignment : OrganizationEntity
{
    public PutawayAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid putawayTaskId,
        Guid binLocationId,
        Guid assignedUserId,
        DateTimeOffset assignedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        PutawayTaskId = DomainRules.Id(putawayTaskId, nameof(putawayTaskId));
        BinLocationId = DomainRules.Id(binLocationId, nameof(binLocationId));
        AssignedUserId = DomainRules.Id(assignedUserId, nameof(assignedUserId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
    }

    public Guid PutawayTaskId { get; }
    public Guid BinLocationId { get; }
    public Guid AssignedUserId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
}
