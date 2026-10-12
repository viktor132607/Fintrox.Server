namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxGroup; tenant scope and input fields validated at creation.</summary>
public sealed class TaxGroup : OrganizationEntity
{
    public TaxGroup(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string groupCode,
        string name,
        Guid jurisdictionId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        GroupCode = DomainRules.Text(groupCode, nameof(groupCode));
        Name = DomainRules.Text(name, nameof(name));
        JurisdictionId = DomainRules.Id(jurisdictionId, nameof(jurisdictionId));
        IsActive = isActive;
    }

    public string GroupCode { get; }
    public string Name { get; }
    public Guid JurisdictionId { get; }
    public bool IsActive { get; }
}
