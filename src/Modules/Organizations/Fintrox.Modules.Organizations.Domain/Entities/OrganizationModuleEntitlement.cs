namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationModuleEntitlement record.</summary>
public sealed class OrganizationModuleEntitlement : OrganizationEntity
{
    public OrganizationModuleEntitlement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        Guid grantedByUserId,
        DateTimeOffset grantedAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        GrantedByUserId = DomainRules.Id(grantedByUserId, nameof(grantedByUserId));
        GrantedAtUtc = DomainRules.Instant(grantedAtUtc, nameof(grantedAtUtc));
        IsActive = isActive;
    }

    public string ModuleCode { get; }
    public Guid GrantedByUserId { get; }
    public DateTimeOffset GrantedAtUtc { get; }
    public bool IsActive { get; }
}
