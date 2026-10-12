namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned ModuleDependency record. Domain invariants enforced at construction.</summary>
public sealed class ModuleDependency : OrganizationEntity
{
    public ModuleDependency(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        string dependsOnModuleCode,
        string minimumVersion,
        bool isOptional)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        DependsOnModuleCode = DomainRules.Text(dependsOnModuleCode, nameof(dependsOnModuleCode));
        MinimumVersion = DomainRules.Text(minimumVersion, nameof(minimumVersion));
        IsOptional = isOptional;
    }

    public string ModuleCode { get; }
    public string DependsOnModuleCode { get; }
    public string MinimumVersion { get; }
    public bool IsOptional { get; }
}
