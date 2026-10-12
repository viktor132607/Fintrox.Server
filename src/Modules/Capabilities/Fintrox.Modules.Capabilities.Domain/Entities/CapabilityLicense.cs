namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityLicense record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityLicense : OrganizationEntity
{
    public CapabilityLicense(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        string licenseKey,
        int seatLimit,
        DateOnly expiresOn,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        LicenseKey = DomainRules.Text(licenseKey, nameof(licenseKey));
        SeatLimit = DomainRules.NonnegativeInt(seatLimit, nameof(seatLimit));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
        IsActive = isActive;
    }

    public Guid CapabilityId { get; }
    public string LicenseKey { get; }
    public int SeatLimit { get; }
    public DateOnly ExpiresOn { get; }
    public bool IsActive { get; }
}
