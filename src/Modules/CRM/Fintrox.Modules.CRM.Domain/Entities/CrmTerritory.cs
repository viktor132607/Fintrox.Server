namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmTerritory record. Domain invariants enforced at construction.</summary>
public sealed class CrmTerritory : OrganizationEntity
{
    public CrmTerritory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string region,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Region = DomainRules.Text(region, nameof(region));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string Region { get; }
    public bool IsActive { get; }
}
