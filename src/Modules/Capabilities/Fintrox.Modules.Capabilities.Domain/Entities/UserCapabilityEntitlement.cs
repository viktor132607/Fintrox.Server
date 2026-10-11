namespace Fintrox.Modules.Capabilities.Domain.Entities;

public sealed class UserCapabilityEntitlement : OrganizationEntity
{
    public UserCapabilityEntitlement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        string capabilityCode)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        CapabilityCode = DomainRules.Text(capabilityCode, nameof(capabilityCode));
    }

    public Guid UserId { get; }
    public string CapabilityCode { get; }
}
