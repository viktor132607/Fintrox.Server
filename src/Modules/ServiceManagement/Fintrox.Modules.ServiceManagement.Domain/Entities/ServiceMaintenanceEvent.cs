namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceMaintenanceEvent domain record. No cross-module navigation properties.</summary>
public sealed class ServiceMaintenanceEvent : OrganizationEntity
{
    public ServiceMaintenanceEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid maintenancePlanId,
        Guid serviceEquipmentAssetId,
        DateOnly performedOn,
        Guid technicianId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        MaintenancePlanId = DomainRules.Id(maintenancePlanId, nameof(maintenancePlanId));
        ServiceEquipmentAssetId = DomainRules.Id(serviceEquipmentAssetId, nameof(serviceEquipmentAssetId));
        PerformedOn = DomainRules.Date(performedOn, nameof(performedOn));
        TechnicianId = DomainRules.Id(technicianId, nameof(technicianId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid MaintenancePlanId { get; }
    public Guid ServiceEquipmentAssetId { get; }
    public DateOnly PerformedOn { get; }
    public Guid TechnicianId { get; }
    public string Status { get; }
}
