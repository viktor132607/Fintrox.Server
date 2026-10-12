namespace Fintrox.Modules.Audit.Domain.Entities;

/// <summary>Tenant-scoped AuditRetentionExecution record; cross-module references require application authorization.</summary>
public sealed class AuditRetentionExecution : OrganizationEntity
{
    public AuditRetentionExecution(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid policyId,
        DateTimeOffset startedAtUtc,
        long removedRecords,
        long retainedRecords,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        PolicyId = DomainRules.Id(policyId, nameof(policyId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        RemovedRecords = DomainRules.NonnegativeLong(removedRecords, nameof(removedRecords));
        RetainedRecords = DomainRules.NonnegativeLong(retainedRecords, nameof(retainedRecords));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid PolicyId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public long RemovedRecords { get; }
    public long RetainedRecords { get; }
    public string Status { get; }
}
