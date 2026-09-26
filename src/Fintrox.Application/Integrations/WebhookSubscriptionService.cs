using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class WebhookSubscriptionService(
    IIntegrationWorkflowRepository repository,
    IWebhookSecretProtector secretProtector,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IWebhookSubscriptionService
{
    public async Task<IReadOnlyList<WebhookSubscriptionResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscriptions = await repository.ListSubscriptionsAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        return subscriptions.Select(Map).ToArray();
    }

    public async Task<WebhookSubscriptionResponse?> GetAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscription = await repository.GetSubscriptionAsync(
            organizationId,
            subscriptionId,
            trackChanges: false,
            cancellationToken);

        return subscription is null ? null : Map(subscription);
    }

    public async Task<WebhookSubscriptionSecretResponse> CreateAsync(
        CreateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var eventTypes = NormalizeEventTypes(request.EventTypes);
        var secret = WebhookSecretUtility.Generate();
        var now = timeProvider.GetUtcNow();

        var subscription = WebhookSubscription.Create(
            organizationId,
            request.Name,
            request.TargetUrl,
            SerializeEventTypes(eventTypes),
            secretProtector.Protect(secret),
            WebhookSecretUtility.Prefix(secret),
            now);

        await repository.AddSubscriptionAsync(
            subscription,
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new WebhookSubscriptionSecretResponse(
            Map(subscription),
            secret);
    }

    public async Task<WebhookSubscriptionResponse?> UpdateAsync(
        Guid subscriptionId,
        UpdateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscription = await repository.GetSubscriptionAsync(
            organizationId,
            subscriptionId,
            trackChanges: true,
            cancellationToken);

        if (subscription is null)
        {
            return null;
        }

        subscription.Update(
            request.Name,
            request.TargetUrl,
            SerializeEventTypes(
                NormalizeEventTypes(request.EventTypes)),
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return Map(subscription);
    }

    public async Task<WebhookSubscriptionSecretResponse?> RotateSecretAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscription = await repository.GetSubscriptionAsync(
            organizationId,
            subscriptionId,
            trackChanges: true,
            cancellationToken);

        if (subscription is null)
        {
            return null;
        }

        var secret = WebhookSecretUtility.Generate();

        subscription.RotateSecret(
            secretProtector.Protect(secret),
            WebhookSecretUtility.Prefix(secret),
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);

        return new WebhookSubscriptionSecretResponse(
            Map(subscription),
            secret);
    }

    public async Task<bool> DeactivateAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscription = await repository.GetSubscriptionAsync(
            organizationId,
            subscriptionId,
            trackChanges: true,
            cancellationToken);

        if (subscription is null)
        {
            return false;
        }

        subscription.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<WebhookSubscriptionResponse?> ActivateAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var subscription = await repository.GetSubscriptionAsync(
            organizationId,
            subscriptionId,
            trackChanges: true,
            cancellationToken);

        if (subscription is null)
        {
            return null;
        }

        subscription.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return Map(subscription);
    }

    public async Task<IReadOnlyList<WebhookDeliveryResponse>> ListDeliveriesAsync(
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        if (normalizedStatus is not null &&
            !Enum.TryParse<WebhookDeliveryStatus>(
                normalizedStatus,
                ignoreCase: true,
                out _))
        {
            throw new ArgumentException(
                "Webhook delivery status is invalid.",
                nameof(status));
        }

        var deliveries = await repository.ListDeliveriesAsync(
            organizationId,
            subscriptionId,
            normalizedStatus,
            cancellationToken);

        return deliveries.Select(MapDelivery).ToArray();
    }

    private static IReadOnlyList<string> NormalizeEventTypes(
        IEnumerable<string>? eventTypes)
    {
        var normalized = (eventTypes ?? [])
            .Select(item => item?.Trim().ToLowerInvariant())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "At least one webhook event type is required.",
                nameof(eventTypes));
        }

        if (normalized.Any(item =>
                item.Length > 120 ||
                item.Contains(',')))
        {
            throw new ArgumentException(
                "Webhook event types cannot exceed 120 characters or contain commas.",
                nameof(eventTypes));
        }

        return normalized;
    }

    private static string SerializeEventTypes(
        IEnumerable<string> eventTypes) =>
        string.Join(',', eventTypes);

    private static IReadOnlyList<string> ParseEventTypes(string eventTypes) =>
        eventTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries);

    private static WebhookSubscriptionResponse Map(
        WebhookSubscription subscription) =>
        new(
            subscription.Id,
            subscription.OrganizationId,
            subscription.Name,
            subscription.TargetUrl,
            ParseEventTypes(subscription.EventTypes),
            subscription.SecretPrefix,
            subscription.IsActive,
            subscription.SecretRotatedAtUtc,
            subscription.CreatedAtUtc,
            subscription.UpdatedAtUtc);

    private static WebhookDeliveryResponse MapDelivery(
        WebhookDelivery delivery) =>
        new(
            delivery.Id,
            delivery.OrganizationId,
            delivery.IntegrationEventId,
            delivery.WebhookSubscriptionId,
            delivery.Status.ToString(),
            delivery.AttemptCount,
            delivery.NextAttemptAtUtc,
            delivery.LastAttemptAtUtc,
            delivery.LastStatusCode,
            delivery.LastError,
            delivery.DeliveredAtUtc,
            delivery.CreatedAtUtc,
            delivery.UpdatedAtUtc);
}
