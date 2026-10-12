namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationErrorEvent domain record; references checked in application layer.</summary>
public sealed class IntegrationErrorEvent : OrganizationEntity
{
    public IntegrationErrorEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string eventCode,
        string errorMessage,
        DateTimeOffset occurredAtUtc,
        string severity)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        EventCode = DomainRules.Text(eventCode, nameof(eventCode));
        ErrorMessage = DomainRules.Text(errorMessage, nameof(errorMessage));
        OccurredAtUtc = DomainRules.Instant(occurredAtUtc, nameof(occurredAtUtc));
        Severity = DomainRules.Text(severity, nameof(severity));
    }

    public Guid ConnectorInstallationId { get; }
    public string EventCode { get; }
    public string ErrorMessage { get; }
    public DateTimeOffset OccurredAtUtc { get; }
    public string Severity { get; }
}
