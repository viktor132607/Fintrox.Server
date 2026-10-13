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

        var transactions = scope.ServiceProvider
            .GetRequiredService<Fintrox.Application.Common.Interfaces.ITransactionRunner>();

        // Claim only when ready to process; reserving a whole batch before
        // HTTP I/O could let leases expire while later items wait.
        for (var index = 0; index < 20; index++)
        {
            var claim = await repository.ClaimNextDueDeliveryAsync(
                timeProvider.GetUtcNow(),
                TimeSpan.FromMinutes(2),
                MaxAttempts,
                cancellationToken);

            if (claim is null)
            {
                break;
            }

            await ProcessDeliveryAsync(
                repository,
                transactions,
                secretProtector,
                claim,
                cancellationToken);
        }
    }

    private async Task ProcessDeliveryAsync(
        IIntegrationWorkflowRepository repository,
        Fintrox.Application.Common.Interfaces.ITransactionRunner transactions,
        IWebhookSecretProtector secretProtector,
        WebhookDeliveryClaim claim,
        CancellationToken cancellationToken)
    {
        // The repository returns a detached snapshot of a lease obtained with
        // a single conditional PostgreSQL UPDATE. No tracked stale entity
        // is ever saved over a newer claim.
        var delivery = claim.Delivery;
        var leaseExpiresAt = delivery.NextAttemptAtUtc;

        if (claim.IsExhausted)
        {
            await FailPermanentlyAsync(
                repository, transactions, delivery, leaseExpiresAt,
                "{}", "Webhook attempt limit reached after an expired lease.",
                statusCode: null, cancellationToken);
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
                repository, transactions, delivery, leaseExpiresAt,
                integrationEvent?.PayloadJson ?? "{}",
                "Webhook event or subscription no longer exists.",
                statusCode: null,
                cancellationToken);
            return;
        }

        if (!subscription.IsActive)
        {
            await FailPermanentlyAsync(
                repository, transactions, delivery, leaseExpiresAt,
                integrationEvent.PayloadJson,
                "Webhook subscription is inactive.",
                statusCode: null,
                cancellationToken);
            return;
        }

        try
        {
            // Recheck existing stored subscriptions too: unsafe legacy URLs
            // are never allowed to reach the network.
            WebhookTargetPolicy.Normalize(subscription.TargetUrl);
        }
        catch (ArgumentException)
        {
            await FailPermanentlyAsync(
                repository, transactions, delivery, leaseExpiresAt,
                integrationEvent.PayloadJson,
                "Webhook destination is blocked by outbound security policy.",
                statusCode: null,
                cancellationToken);
            return;
        }

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
                await repository.TryPersistClaimedDeliveryAsync(
                    delivery, leaseExpiresAt, cancellationToken);
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            await HandleFailureAsync(
                repository, transactions, delivery, leaseExpiresAt,
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
        catch (HttpRequestException exception)
            when (IsBlockedDestination(exception))
        {
            // HttpClient may wrap exceptions thrown by ConnectCallback.
            await FailPermanentlyAsync(
                repository, transactions, delivery, leaseExpiresAt,
                integrationEvent.PayloadJson,
                "Webhook DNS resolution returned a blocked destination.",
                statusCode: null,
                cancellationToken);
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(
                repository, transactions, delivery, leaseExpiresAt,
                integrationEvent.PayloadJson,
                statusCode: null,
                exception.Message,
                cancellationToken);
        }
    }

    private async Task HandleFailureAsync(
        IIntegrationWorkflowRepository repository,
        Fintrox.Application.Common.Interfaces.ITransactionRunner transactions,
        WebhookDelivery delivery,
        DateTimeOffset leaseExpiresAt,
        string payloadJson,
        int? statusCode,
        string error,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (delivery.AttemptCount >= MaxAttempts)
        {
            await FailPermanentlyAsync(
                repository, transactions, delivery, leaseExpiresAt,
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

        await repository.TryPersistClaimedDeliveryAsync(
            delivery, leaseExpiresAt, cancellationToken);
    }

    private async Task FailPermanentlyAsync(
        IIntegrationWorkflowRepository repository,
        Fintrox.Application.Common.Interfaces.ITransactionRunner transactions,
        WebhookDelivery delivery,
        DateTimeOffset leaseExpiresAt,
        string payloadJson,
        string error,
        int? statusCode,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        delivery.Fail(statusCode, error, now);

        await transactions.ExecuteAsync(async token =>
        {
            // A stale worker cannot overwrite a claim recovered by another
            // worker. The failure record is committed atomically with the
            // winning terminal state transition.
            if (!await repository.TryPersistClaimedDeliveryAsync(
                    delivery, leaseExpiresAt, token))
            {
                return false;
            }

            var failure = await repository.GetOpenFailureAsync(
                delivery.OrganizationId,
                IntegrationFailureKind.WebhookDelivery,
                delivery.Id,
                trackChanges: true,
                token);

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
                    token);
            }
            else
            {
                failure.RecordRetry(error, payloadJson, now);
            }

            await repository.SaveChangesAsync(token);
            return true;
        }, cancellationToken);
    }

    private static bool IsBlockedDestination(Exception exception)
    {
        for (Exception? inner = exception; inner is not null; inner = inner.InnerException)
        {
            if (inner is WebhookTargetBlockedException)
            {
                return true;
            }
        }

        return false;
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
