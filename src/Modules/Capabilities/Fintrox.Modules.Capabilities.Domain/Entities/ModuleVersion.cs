namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned ModuleVersion record. Domain invariants enforced at construction.</summary>
public sealed class ModuleVersion : OrganizationEntity
{
    public ModuleVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        string semanticVersion,
        DateOnly releasedOn,
        string releaseChannel)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        SemanticVersion = DomainRules.Text(semanticVersion, nameof(semanticVersion));
        ReleasedOn = DomainRules.Date(releasedOn, nameof(releasedOn));
        ReleaseChannel = DomainRules.Text(releaseChannel, nameof(releaseChannel));
    }

    public string ModuleCode { get; }
    public string SemanticVersion { get; }
    public DateOnly ReleasedOn { get; }
    public string ReleaseChannel { get; }
}
