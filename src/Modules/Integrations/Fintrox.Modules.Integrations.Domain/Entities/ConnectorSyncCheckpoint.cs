namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned ConnectorSyncCheckpoint domain record; references checked in application layer.</summary>
public sealed class ConnectorSyncCheckpoint : OrganizationEntity
{
    public ConnectorSyncCheckpoint(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string cursorValue,
        DateTimeOffset synchronizedAtUtc,
        long recordCount)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        CursorValue = DomainRules.Text(cursorValue, nameof(cursorValue));
        SynchronizedAtUtc = DomainRules.Instant(synchronizedAtUtc, nameof(synchronizedAtUtc));
        RecordCount = DomainRules.NonnegativeLong(recordCount, nameof(recordCount));
    }

    public Guid ConnectorInstallationId { get; }
    public string CursorValue { get; }
    public DateTimeOffset SynchronizedAtUtc { get; }
    public long RecordCount { get; }
}
