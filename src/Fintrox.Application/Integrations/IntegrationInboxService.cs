using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class IntegrationInboxService(
    IIntegrationWorkflowRepository repository,
    IIntegrationBusinessEventDispatcher dispatcher,
    IIntegrationOutboxService outbox,
    ITransactionRunner transactionRunner,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IIntegrationInboxService
{
    public async Task<IntegrationInboxResponse> ReceiveAsync(
        IntegrationInboxCommand command,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var sourceSystem = command.SourceSystem.Trim().ToLowerInvariant();
        var externalId = command.ExternalId.Trim();
        var eventType = command.EventType.Trim().ToLowerInvariant();
        var operation = command.Operation.Trim().ToLowerInvariant();

        if (!IntegrationBusinessOperations.All.Contains(
                operation,
                StringComparer.Ordinal))
        {
            throw new IntegrationProcessingException(
                $"Unsupported integration business operation '{command.Operation}'.");
        }

        var existing = await repository.GetInboxByExternalKeyAsync(
            organizationId,
            sourceSystem,
            externalId,
            eventType,
            trackChanges: false,
            cancellationToken);

        if (existing is not null)
        {
            return Map(existing);
        }

        var inbox = IntegrationInbox.Create(
            organizationId,
            sourceSystem,
            externalId,
            eventType,
            operation,
            command.PayloadJson,
            timeProvider.GetUtcNow());

        await repository.AddInboxAsync(
            inbox,
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ProcessAsync(
            organizationId,
            inbox.Id,
            cancellationToken);
    }

    public async Task<IntegrationInboxResponse?> RetryAsync(
        Guid inboxId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var inbox = await repository.GetInboxAsync(
            organizationId,
            inboxId,
            trackChanges: false,
            cancellationToken);

        if (inbox is null)
        {
            return null;
        }

        if (inbox.Status == IntegrationInboxStatus.Succeeded)
        {
            return Map(inbox);
        }

        return await ProcessAsync(
            organizationId,
            inboxId,
            cancellationToken);
    }

    public async Task<IntegrationInboxResponse?> GetAsync(
        Guid inboxId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var inbox = await repository.GetInboxAsync(
            organizationId,
            inboxId,
            trackChanges: false,
            cancellationToken);

        return inbox is null ? null : Map(inbox);
    }

    public async Task<IReadOnlyList<IntegrationInboxResponse>> ListAsync(
        string? status,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : status.Trim();

        if (normalizedStatus is not null &&
            !Enum.TryParse<IntegrationInboxStatus>(
                normalizedStatus,
                ignoreCase: true,
                out _))
        {
            throw new ArgumentException(
                "Inbox status is invalid.",
                nameof(status));
        }

        var items = await repository.ListInboxAsync(
            organizationId,
            normalizedStatus,
            cancellationToken);

        return items.Select(Map).ToArray();
    }

    private async Task<IntegrationInboxResponse> ProcessAsync(
        Guid organizationId,
        Guid inboxId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await transactionRunner.ExecuteAsync(
                async ct =>
                {
                    var inbox = await repository.GetInboxAsync(
                        organizationId,
                        inboxId,
                        trackChanges: true,
                        ct)
                        ?? throw new IntegrationProcessingException(
                            "Integration inbox item no longer exists.");

                    inbox.StartProcessing(timeProvider.GetUtcNow());
                    await repository.SaveChangesAsync(ct);

                    var result = await dispatcher.DispatchAsync(
                        inbox.Operation,
                        inbox.PayloadJson,
                        ct);

                    inbox.Succeed(
                        result.AggregateType,
                        result.AggregateId,
                        result.JournalEntryId,
                        timeProvider.GetUtcNow());

                    await outbox.EnqueueAsync(
                        organizationId,
                        inbox.Id,
                        result,
                        ct);

                    var openFailure = await repository.GetOpenFailureAsync(
                        organizationId,
                        IntegrationFailureKind.InboxProcessing,
                        inbox.Id,
                        trackChanges: true,
                        ct);

                    openFailure?.Resolve(timeProvider.GetUtcNow());

                    await repository.SaveChangesAsync(ct);
                    return Map(inbox);
                },
                cancellationToken);
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException)
        {
            var failed = await repository.GetInboxAsync(
                organizationId,
                inboxId,
                trackChanges: true,
                cancellationToken)
                ?? throw;

            var now = timeProvider.GetUtcNow();
            failed.Fail(
                exception.Message,
                now);

            var failure = await repository.GetOpenFailureAsync(
                organizationId,
                IntegrationFailureKind.InboxProcessing,
                failed.Id,
                trackChanges: true,
                cancellationToken);

            if (failure is null)
            {
                await repository.AddFailureAsync(
                    IntegrationFailure.Create(
                        organizationId,
                        IntegrationFailureKind.InboxProcessing,
                        failed.Id,
                        exception.Message,
                        failed.PayloadJson,
                        now),
                    cancellationToken);
            }
            else
            {
                failure.RecordRetry(
                    exception.Message,
                    failed.PayloadJson,
                    now);
            }

            await repository.SaveChangesAsync(cancellationToken);
            return Map(failed);
        }
    }

    private static IntegrationInboxResponse Map(
        IntegrationInbox inbox) =>
        new(
            inbox.Id,
            inbox.OrganizationId,
            inbox.SourceSystem,
            inbox.ExternalId,
            inbox.EventType,
            inbox.Operation,
            inbox.PayloadJson,
            inbox.Status.ToString(),
            inbox.RetryCount,
            inbox.LastAttemptAtUtc,
            inbox.CompletedAtUtc,
            inbox.ResultEntityType,
            inbox.ResultEntityId,
            inbox.JournalEntryId,
            inbox.FailureReason,
            inbox.CreatedAtUtc,
            inbox.UpdatedAtUtc);
}
