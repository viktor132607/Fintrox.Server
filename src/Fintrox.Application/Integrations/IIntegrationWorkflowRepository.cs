using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationWorkflowRepository
{
    Task<IntegrationInbox?> GetInboxByExternalKeyAsync(
        Guid organizationId,
        string sourceSystem,
        string externalId,
        string eventType,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IntegrationInbox?> GetInboxAsync(
        Guid organizationId,
        Guid inboxId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<IntegrationInbox>> ListInboxAsync(
        Guid organizationId,
        string? status,
        CancellationToken cancellationToken);

    Task AddInboxAsync(
        IntegrationInbox inbox,
        CancellationToken cancellationToken);

    Task AddEventAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken);

    Task<IntegrationEvent?> GetEventAsync(
        Guid eventId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookSubscription>> ListSubscriptionsAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookSubscription>> ListMatchingSubscriptionsAsync(
        Guid organizationId,
        string eventType,
        CancellationToken cancellationToken);

    Task<WebhookSubscription?> GetSubscriptionAsync(
        Guid organizationId,
        Guid subscriptionId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task AddSubscriptionAsync(
        WebhookSubscription subscription,
        CancellationToken cancellationToken);

    Task AddDeliveryAsync(
        WebhookDelivery delivery,
        CancellationToken cancellationToken);

    Task<WebhookDelivery?> GetDeliveryAsync(
        Guid organizationId,
        Guid deliveryId,
        bool trackChanges,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Atomically reserves a due delivery before network I/O. A lease is
    /// recoverable after its deadline; claims are global across API instances.
    /// </summary>
    Task<WebhookDeliveryClaim?> ClaimNextDueDeliveryAsync(
        DateTimeOffset now,
        TimeSpan leaseDuration,
        int maxAttempts,
        CancellationToken cancellationToken);

    /// <summary>
    /// Writes a result only if the caller still holds the claimed attempt
    /// generation and original lease deadline. False means lost ownership.
    /// </summary>
    Task<bool> TryPersistClaimedDeliveryAsync(
        WebhookDelivery delivery,
        DateTimeOffset claimedLeaseExpiresAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookDelivery>> ListDeliveriesAsync(
        Guid organizationId,
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookDelivery>> ListDueDeliveriesAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken);

    Task<IntegrationFailure?> GetOpenFailureAsync(
        Guid organizationId,
        IntegrationFailureKind kind,
        Guid referenceId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IntegrationFailure?> GetFailureAsync(
        Guid organizationId,
        Guid failureId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<IntegrationFailure>> ListFailuresAsync(
        Guid organizationId,
        bool includeResolved,
        CancellationToken cancellationToken);

    Task AddFailureAsync(
        IntegrationFailure failure,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
