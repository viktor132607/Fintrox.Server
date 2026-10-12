namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityRollout record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityRollout : OrganizationEntity
{
    public CapabilityRollout(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        string environment,
        decimal exposurePercent,
        DateOnly startsOn,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        Environment = DomainRules.Text(environment, nameof(environment));
        ExposurePercent = DomainRules.Percent(exposurePercent, nameof(exposurePercent));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        IsEnabled = isEnabled;
    }

    public Guid CapabilityId { get; }
    public string Environment { get; }
    public decimal ExposurePercent { get; }
    public DateOnly StartsOn { get; }
    public bool IsEnabled { get; }
}
