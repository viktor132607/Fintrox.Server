namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportDataset model. References require application validation.</summary>
public sealed class ReportDataset : OrganizationEntity
{
    public ReportDataset(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string ownerModuleCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        OwnerModuleCode = DomainRules.Text(ownerModuleCode, nameof(ownerModuleCode));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string OwnerModuleCode { get; }
    public bool IsActive { get; }
}
