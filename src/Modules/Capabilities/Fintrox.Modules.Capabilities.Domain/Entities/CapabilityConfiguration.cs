namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityConfiguration record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityConfiguration : OrganizationEntity
{
    public CapabilityConfiguration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        string configurationJson,
        int versionNumber,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        ConfigurationJson = DomainRules.Json(configurationJson, nameof(configurationJson));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        IsActive = isActive;
    }

    public Guid CapabilityId { get; }
    public string ConfigurationJson { get; }
    public int VersionNumber { get; }
    public bool IsActive { get; }
}
