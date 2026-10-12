namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityActivationHistory record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityActivationHistory : OrganizationEntity
{
    public CapabilityActivationHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        Guid activatedByUserId,
        DateTimeOffset changedAtUtc,
        string previousState,
        string newState)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        ActivatedByUserId = DomainRules.Id(activatedByUserId, nameof(activatedByUserId));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
        PreviousState = DomainRules.Text(previousState, nameof(previousState));
        NewState = DomainRules.Text(newState, nameof(newState));
    }

    public Guid CapabilityId { get; }
    public Guid ActivatedByUserId { get; }
    public DateTimeOffset ChangedAtUtc { get; }
    public string PreviousState { get; }
    public string NewState { get; }
}
