namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned ModuleProvisioningJob record. Domain invariants enforced at construction.</summary>
public sealed class ModuleProvisioningJob : OrganizationEntity
{
    public ModuleProvisioningJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string moduleCode,
        Guid requestedByUserId,
        DateTimeOffset requestedAtUtc,
        string status,
        int attemptCount)
        : base(id, organizationId, createdAtUtc)
    {
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
        AttemptCount = DomainRules.NonnegativeInt(attemptCount, nameof(attemptCount));
    }

    public string ModuleCode { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
    public int AttemptCount { get; }
}
