namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationRetryPolicy domain record; references checked in application layer.</summary>
public sealed class IntegrationRetryPolicy : OrganizationEntity
{
    public IntegrationRetryPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        int maxAttempts,
        int retryDelaySeconds,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        MaxAttempts = DomainRules.PositiveInt(maxAttempts, nameof(maxAttempts));
        RetryDelaySeconds = DomainRules.PositiveInt(retryDelaySeconds, nameof(retryDelaySeconds));
        IsEnabled = isEnabled;
    }

    public Guid ConnectorInstallationId { get; }
    public int MaxAttempts { get; }
    public int RetryDelaySeconds { get; }
    public bool IsEnabled { get; }
}
