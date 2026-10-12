namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityUsage record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityUsage : OrganizationEntity
{
    public CapabilityUsage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        DateOnly usageDate,
        long consumedUnits,
        string unitName)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        UsageDate = DomainRules.Date(usageDate, nameof(usageDate));
        ConsumedUnits = DomainRules.NonnegativeLong(consumedUnits, nameof(consumedUnits));
        UnitName = DomainRules.Text(unitName, nameof(unitName));
    }

    public Guid CapabilityId { get; }
    public DateOnly UsageDate { get; }
    public long ConsumedUnits { get; }
    public string UnitName { get; }
}
