using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fintrox.Application.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Api.Integrations;

public sealed class WebhookDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider,
    ILogger<WebhookDeliveryWorker> logger) : BackgroundService
{
    private const int MaxAttempts = 6;

    private static readonly Action<ILogger, Exception?> LogBatchFailure =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2101, nameof(WebhookDeliveryWorker)),
            "Webhook delivery batch failed.");
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(6)
    ];

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogBatchFailure(
                    logger,
                    exception);
            }

            await Task.Delay(
                TimeSpan.FromSeconds(10),
                timeProvider,
                stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider
            .GetRequiredService<IIntegrationWorkflowRepository>();
        var secretProtector = scope.ServiceProvider
            .GetRequiredService<IWebhookSecretProtector>();

        var due = await repository.ListDueDeliveriesAsync(
            timeProvider.GetUtcNow(),
            take: 20,
            cancellationToken);

        foreach (var candidate in due)
        {
            await ProcessDeliveryAsync(
                repository,
                secretProtector,
                candidate,
                cancellationToken);
        }
    }

    private async Task ProcessDeliveryAsync(
        IIntegrationWorkflowRepository repository,
        IWebhookSecretProtector secretProtector,
        WebhookDelivery candidate,
        CancellationToken cancellationToken)
    {
        var delivery = await repository.GetDeliveryAsync(
            candidate.OrganizationId,
            candidate.Id,
            trackChanges: true,
            cancellationToken);

        if (delivery is null ||
            delivery.Status != WebhookDeliveryStatus.Pending ||
            delivery.NextAttemptAtUtc > timeProvider.GetUtcNow())
        {
            return;
        }

        var integrationEvent = await repository.GetEventAsync(
            delivery.IntegrationEventId,
            trackChanges: false,
            cancellationToken);

        var subscription = await repository.GetSubscriptionAsync(
            delivery.OrganizationId,
            delivery.WebhookSubscriptionId,
            trackChanges: false,
            cancellationToken);

        if (integrationEvent is null || subscription is null)
        {
            await FailPermanentlyAsync(
                repository,
                delivery,
                integrationEvent?.PayloadJson ?? "{}",
                "Webhook event or subscription no longer exists.",
                statusCode: null,
                cancellationToken);
            return;
        }

        if (!subscription.IsActive)
        {
            await FailPermanentlyAsync(
                repository,
                delivery,
                integrationEvent.PayloadJson,
                "Webhook subscription is inactive.",
                statusCode: null,
                cancellationToken);
            return;
        }

        var now = timeProvider.GetUtcNow();
        delivery.StartAttempt(now);
        await repository.SaveChangesAsync(cancellationToken);

        var body = BuildBody(
            integrationEvent,
            delivery);
        var secret = secretProtector.Unprotect(
            subscription.SigningSecretCiphertext);
        var signature = Sign(body, secret);

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                subscription.TargetUrl)
            {
                Content = new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add(
                "X-Fintrox-Event-Id",
                integrationEvent.Id.ToString());
            request.Headers.Add(
                "X-Fintrox-Delivery-Id",
                delivery.Id.ToString());
            request.Headers.Add(
                "X-Fintrox-Signature",
                $"sha256={signature}");

            var client = httpClientFactory.CreateClient(
                "Fintrox.Webhooks");

            using var response = await client.SendAsync(
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                delivery.Succeed(
                    (int)response.StatusCode,
                    timeProvider.GetUtcNow());
                await repository.SaveChangesAsync(cancellationToken);
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            await HandleFailureAsync(
                repository,
                delivery,
                integrationEvent.PayloadJson,
                (int)response.StatusCode,
                $"Webhook endpoint returned HTTP {(int)response.StatusCode}: {responseBody}",
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(
                repository,
                delivery,
                integrationEvent.PayloadJson,
                statusCode: null,
                exception.Message,
                cancellationToken);
        }
    }

    private async Task HandleFailureAsync(
        IIntegrationWorkflowRepository repository,
        WebhookDelivery delivery,
        string payloadJson,
        int? statusCode,
        string error,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (delivery.AttemptCount >= MaxAttempts)
        {
            await FailPermanentlyAsync(
                repository,
                delivery,
                payloadJson,
                error,
                statusCode,
                cancellationToken);
            return;
        }

        var delayIndex = Math.Min(
            delivery.AttemptCount - 1,
            RetryDelays.Length - 1);

        delivery.ScheduleRetry(
            statusCode,
            error,
            now.Add(RetryDelays[delayIndex]),
            now);

        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task FailPermanentlyAsync(
        IIntegrationWorkflowRepository repository,
        WebhookDelivery delivery,
        string payloadJson,
        string error,
        int? statusCode,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (delivery.Status == WebhookDeliveryStatus.Pending)
        {
            if (delivery.LastAttemptAtUtc is null)
            {
                delivery.StartAttempt(now);
            }

            delivery.Fail(
                statusCode,
                error,
                now);
        }

        var failure = await repository.GetOpenFailureAsync(
            delivery.OrganizationId,
            IntegrationFailureKind.WebhookDelivery,
            delivery.Id,
            trackChanges: true,
            cancellationToken);

        if (failure is null)
        {
            await repository.AddFailureAsync(
                IntegrationFailure.Create(
                    delivery.OrganizationId,
                    IntegrationFailureKind.WebhookDelivery,
                    delivery.Id,
                    error,
                    payloadJson,
                    now),
                cancellationToken);
        }
        else
        {
            failure.RecordRetry(
                error,
                payloadJson,
                now);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }

    private static string BuildBody(
        IntegrationEvent integrationEvent,
        WebhookDelivery delivery)
    {
        using var payload = JsonDocument.Parse(
            integrationEvent.PayloadJson);

        return JsonSerializer.Serialize(new
        {
            id = integrationEvent.Id,
            deliveryId = delivery.Id,
            type = integrationEvent.EventType,
            organizationId = integrationEvent.OrganizationId,
            aggregateType = integrationEvent.AggregateType,
            aggregateId = integrationEvent.AggregateId,
            occurredAtUtc = integrationEvent.OccurredAtUtc,
            data = payload.RootElement.Clone()
        });
    }

    private static string Sign(
        string body,
        string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(body);

        return Convert.ToHexString(
                HMACSHA256.HashData(
                    key,
                    data))
            .ToLowerInvariant();
    }
}
