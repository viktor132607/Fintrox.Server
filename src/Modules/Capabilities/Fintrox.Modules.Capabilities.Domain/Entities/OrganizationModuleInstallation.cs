namespace Fintrox.Modules.Capabilities.Domain.Entities;

public sealed class OrganizationModuleInstallation : OrganizationEntity
{
    public OrganizationModuleInstallation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        string version)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        Version = DomainRules.Text(version, nameof(version));
    }

    public string ModuleCode { get; }
    public string Version { get; }
}
