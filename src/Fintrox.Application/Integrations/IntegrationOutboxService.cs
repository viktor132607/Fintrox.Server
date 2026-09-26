using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class IntegrationOutboxService(
    IIntegrationWorkflowRepository repository,
    TimeProvider timeProvider) : IIntegrationOutboxService
{
    public async Task<IntegrationEvent> EnqueueAsync(
        Guid organizationId,
        Guid? inboxId,
        IntegrationBusinessEventResult result,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var integrationEvent = IntegrationEvent.Create(
            organizationId,
            inboxId,
            result.OutboxEventType,
            result.AggregateType,
            result.AggregateId,
            result.PayloadJson,
            now,
            now);

        await repository.AddEventAsync(
            integrationEvent,
            cancellationToken);

        var subscriptions = await repository.ListMatchingSubscriptionsAsync(
            organizationId,
            result.OutboxEventType,
            cancellationToken);

        foreach (var subscription in subscriptions)
        {
            await repository.AddDeliveryAsync(
                WebhookDelivery.Create(
                    organizationId,
                    integrationEvent.Id,
                    subscription.Id,
                    now),
                cancellationToken);
        }

        integrationEvent.MarkDispatched(now);
        return integrationEvent;
    }
}
