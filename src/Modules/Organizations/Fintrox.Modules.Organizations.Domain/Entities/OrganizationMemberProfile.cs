namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationMemberProfile record.</summary>
public sealed class OrganizationMemberProfile : OrganizationEntity
{
    public OrganizationMemberProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid employeeId,
        string displayName,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        DisplayName = DomainRules.Text(displayName, nameof(displayName));
        IsActive = isActive;
    }

    public Guid UserId { get; }
    public Guid EmployeeId { get; }
    public string DisplayName { get; }
    public bool IsActive { get; }
}
