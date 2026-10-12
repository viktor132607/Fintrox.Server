namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeeOrgHistory domain model.</summary>
public sealed class EmployeeOrgHistory : OrganizationEntity
{
    public EmployeeOrgHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        Guid departmentId,
        Guid managerEmployeeId,
        DateOnly effectiveFrom,
        DateOnly effectiveTo)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        DepartmentId = DomainRules.Id(departmentId, nameof(departmentId));
        ManagerEmployeeId = DomainRules.Id(managerEmployeeId, nameof(managerEmployeeId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        EffectiveTo = DomainRules.Date(effectiveTo, nameof(effectiveTo));
        if (EffectiveTo < EffectiveFrom) throw new ArgumentException("End must not precede start.", nameof(effectiveTo));
    }

    public Guid EmployeeId { get; }
    public Guid DepartmentId { get; }
    public Guid ManagerEmployeeId { get; }
    public DateOnly EffectiveFrom { get; }
    public DateOnly EffectiveTo { get; }
}
