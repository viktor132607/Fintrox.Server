namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned EmployeePosition domain model.</summary>
public sealed class EmployeePosition : OrganizationEntity
{
    public EmployeePosition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string positionCode,
        string title,
        Guid departmentId,
        Guid reportsToPositionId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        PositionCode = DomainRules.Text(positionCode, nameof(positionCode));
        Title = DomainRules.Text(title, nameof(title));
        DepartmentId = DomainRules.Id(departmentId, nameof(departmentId));
        ReportsToPositionId = DomainRules.Id(reportsToPositionId, nameof(reportsToPositionId));
        IsActive = isActive;
    }

    public string PositionCode { get; }
    public string Title { get; }
    public Guid DepartmentId { get; }
    public Guid ReportsToPositionId { get; }
    public bool IsActive { get; }
}
