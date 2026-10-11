namespace Fintrox.Modules.Capabilities.Domain.Entities;

public sealed class OrganizationCapabilityActivation : OrganizationEntity
{
    public OrganizationCapabilityActivation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string capabilityCode)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
    }

    public string CapabilityCode { get; }
}
