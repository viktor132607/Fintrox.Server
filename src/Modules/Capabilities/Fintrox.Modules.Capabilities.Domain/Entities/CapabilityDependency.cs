namespace Fintrox.Modules.Capabilities.Domain.Entities;

public sealed class CapabilityDependency : OrganizationEntity
{
    public CapabilityDependency(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        Guid requiredCapabilityId)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        RequiredCapabilityId = DomainRules.Id(requiredCapabilityId, nameof(requiredCapabilityId));
        if (CapabilityId == RequiredCapabilityId) throw new ArgumentException("A capability cannot depend on itself.", nameof(requiredCapabilityId));
    }

    public Guid CapabilityId { get; }
    public Guid RequiredCapabilityId { get; }
}
