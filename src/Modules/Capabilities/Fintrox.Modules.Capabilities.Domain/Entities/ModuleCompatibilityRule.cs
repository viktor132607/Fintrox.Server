namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned ModuleCompatibilityRule record. Domain invariants enforced at construction.</summary>
public sealed class ModuleCompatibilityRule : OrganizationEntity
{
    public ModuleCompatibilityRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        string requiredModuleCode,
        string minimumVersion,
        bool isMandatory)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        RequiredModuleCode = DomainRules.Text(requiredModuleCode, nameof(requiredModuleCode));
        MinimumVersion = DomainRules.Text(minimumVersion, nameof(minimumVersion));
        IsMandatory = isMandatory;
    }

    public string ModuleCode { get; }
    public string RequiredModuleCode { get; }
    public string MinimumVersion { get; }
    public bool IsMandatory { get; }
}
