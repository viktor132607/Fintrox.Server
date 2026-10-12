namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned WebhookSubscriptionConfig domain record; references checked in application layer.</summary>
public sealed class WebhookSubscriptionConfig : OrganizationEntity
{
    public WebhookSubscriptionConfig(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string callbackUri,
        string eventName,
        string secretReference,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        CallbackUri = DomainRules.Text(callbackUri, nameof(callbackUri));
        EventName = DomainRules.Text(eventName, nameof(eventName));
        SecretReference = DomainRules.Text(secretReference, nameof(secretReference));
        IsActive = isActive;
    }

    public Guid ConnectorInstallationId { get; }
    public string CallbackUri { get; }
    public string EventName { get; }
    public string SecretReference { get; }
    public bool IsActive { get; }
}
