namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorSyncJob domain record; references checked in application layer.</summary>
public sealed class ConnectorSyncJob : OrganizationEntity
{
    public ConnectorSyncJob(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        DateTimeOffset startedAtUtc,
        string status,
        int processedCount)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
        ProcessedCount = DomainRules.NonnegativeInt(processedCount, nameof(processedCount));
    }

    public Guid ConnectorInstallationId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public string Status { get; }
    public int ProcessedCount { get; }
}
