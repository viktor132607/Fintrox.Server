namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankFeedSyncJob record; cross-module references require application authorization.</summary>
public sealed class BankFeedSyncJob : OrganizationEntity
{
    public BankFeedSyncJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectionId,
        DateTimeOffset startedAtUtc,
        string status,
        int importedCount)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectionId = DomainRules.Id(connectionId, nameof(connectionId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
        ImportedCount = DomainRules.NonnegativeInt(importedCount, nameof(importedCount));
    }

    public Guid ConnectionId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public string Status { get; }
    public int ImportedCount { get; }
}
