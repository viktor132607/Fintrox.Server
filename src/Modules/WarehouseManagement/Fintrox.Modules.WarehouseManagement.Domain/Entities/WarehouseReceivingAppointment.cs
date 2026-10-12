namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseReceivingAppointment; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseReceivingAppointment : OrganizationEntity
{
    public WarehouseReceivingAppointment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        Guid supplierId,
        DateTimeOffset scheduledAtUtc,
        string reference,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        ScheduledAtUtc = DomainRules.Instant(scheduledAtUtc, nameof(scheduledAtUtc));
        Reference = DomainRules.Text(reference, nameof(reference));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public Guid SupplierId { get; }
    public DateTimeOffset ScheduledAtUtc { get; }
    public string Reference { get; }
    public string Status { get; }
}
