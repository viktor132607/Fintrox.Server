namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityOverride record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityOverride : OrganizationEntity
{
    public CapabilityOverride(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        Guid userId,
        string reason,
        DateOnly effectiveFrom,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        UserId = DomainRules.Id(userId, nameof(userId));
        Reason = DomainRules.Text(reason, nameof(reason));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsEnabled = isEnabled;
    }

    public Guid CapabilityId { get; }
    public Guid UserId { get; }
    public string Reason { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsEnabled { get; }
}
