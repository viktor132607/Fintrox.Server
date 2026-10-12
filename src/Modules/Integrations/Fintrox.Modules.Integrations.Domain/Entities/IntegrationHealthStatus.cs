namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationHealthStatus domain record; references checked in application layer.</summary>
public sealed class IntegrationHealthStatus : OrganizationEntity
{
    public IntegrationHealthStatus(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        DateTimeOffset checkedAtUtc,
        string healthCode,
        int latencyMilliseconds)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        CheckedAtUtc = DomainRules.Instant(checkedAtUtc, nameof(checkedAtUtc));
        HealthCode = DomainRules.Text(healthCode, nameof(healthCode));
        LatencyMilliseconds = DomainRules.NonnegativeInt(latencyMilliseconds, nameof(latencyMilliseconds));
    }

    public Guid ConnectorInstallationId { get; }
    public DateTimeOffset CheckedAtUtc { get; }
    public string HealthCode { get; }
    public int LatencyMilliseconds { get; }
}
