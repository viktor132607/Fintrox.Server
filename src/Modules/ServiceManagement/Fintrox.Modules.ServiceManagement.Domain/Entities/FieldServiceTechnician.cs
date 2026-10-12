namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped FieldServiceTechnician domain record. No cross-module navigation properties.</summary>
public sealed class FieldServiceTechnician : OrganizationEntity
{
    public FieldServiceTechnician(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string skillCode,
        string serviceRegion,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        SkillCode = DomainRules.Text(skillCode, nameof(skillCode));
        ServiceRegion = DomainRules.Text(serviceRegion, nameof(serviceRegion));
        IsActive = isActive;
    }

    public Guid EmployeeId { get; }
    public string SkillCode { get; }
    public string ServiceRegion { get; }
    public bool IsActive { get; }
}
