using Fintrox.Contracts.Integrations;

namespace Fintrox.Application.Integrations;

public interface IWebhookSubscriptionService
{
    Task<IReadOnlyList<WebhookSubscriptionResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<WebhookSubscriptionResponse?> GetAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken);

    Task<WebhookSubscriptionSecretResponse> CreateAsync(
        CreateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task<WebhookSubscriptionResponse?> UpdateAsync(
        Guid subscriptionId,
        UpdateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task<WebhookSubscriptionSecretResponse?> RotateSecretAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken);

    Task<WebhookSubscriptionResponse?> ActivateAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WebhookDeliveryResponse>> ListDeliveriesAsync(
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken);
}
