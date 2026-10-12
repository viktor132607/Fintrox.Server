namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned FeatureFlagOverride record. Domain invariants enforced at construction.</summary>
public sealed class FeatureFlagOverride : OrganizationEntity
{
    public FeatureFlagOverride(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid featureFlagId,
        string targetScope,
        Guid targetId,
        bool isEnabled,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        FeatureFlagId = DomainRules.Id(featureFlagId, nameof(featureFlagId));
        TargetScope = DomainRules.Text(targetScope, nameof(targetScope));
        TargetId = DomainRules.Id(targetId, nameof(targetId));
        IsEnabled = isEnabled;
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid FeatureFlagId { get; }
    public string TargetScope { get; }
    public Guid TargetId { get; }
    public bool IsEnabled { get; }
    public DateOnly EffectiveFrom { get; }
}
