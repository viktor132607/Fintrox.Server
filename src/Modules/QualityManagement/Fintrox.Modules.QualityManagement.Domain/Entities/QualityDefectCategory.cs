namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityDefectCategory model. References require application validation.</summary>
public sealed class QualityDefectCategory : OrganizationEntity
{
    public QualityDefectCategory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string severity,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Severity = DomainRules.Text(severity, nameof(severity));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string Severity { get; }
    public bool IsActive { get; }
}
