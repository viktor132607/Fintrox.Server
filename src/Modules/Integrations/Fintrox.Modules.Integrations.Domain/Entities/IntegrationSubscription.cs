namespace Fintrox.Modules.Integrations.Domain.Entities;

/// <summary>Immutable tenant-owned IntegrationSubscription domain record; references checked in application layer.</summary>
public sealed class IntegrationSubscription : OrganizationEntity
{
    public IntegrationSubscription(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid connectorInstallationId,
        string subscriptionName,
        string eventType,
        DateTimeOffset subscribedAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ConnectorInstallationId = DomainRules.Id(connectorInstallationId, nameof(connectorInstallationId));
        SubscriptionName = DomainRules.Text(subscriptionName, nameof(subscriptionName));
        EventType = DomainRules.Text(eventType, nameof(eventType));
        SubscribedAtUtc = DomainRules.Instant(subscribedAtUtc, nameof(subscribedAtUtc));
        IsActive = isActive;
    }

    public Guid ConnectorInstallationId { get; }
    public string SubscriptionName { get; }
    public string EventType { get; }
    public DateTimeOffset SubscribedAtUtc { get; }
    public bool IsActive { get; }
}
