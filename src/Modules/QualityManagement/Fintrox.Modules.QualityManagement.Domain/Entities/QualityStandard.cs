namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityStandard model. References require application validation.</summary>
public sealed class QualityStandard : OrganizationEntity
{
    public QualityStandard(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string standardVersion,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        StandardVersion = DomainRules.Text(standardVersion, nameof(standardVersion));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string StandardVersion { get; }
    public bool IsActive { get; }
}
