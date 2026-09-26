using Fintrox.Application.Integrations;
using Fintrox.Domain.Integrations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Integrations;

public sealed class IntegrationWorkflowRepository(
    FintroxDbContext dbContext) : IIntegrationWorkflowRepository
{
    public Task<IntegrationInbox?> GetInboxByExternalKeyAsync(
        Guid organizationId,
        string sourceSystem,
        string externalId,
        string eventType,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationInbox> query = dbContext.IntegrationInbox;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.SourceSystem == sourceSystem &&
                item.ExternalId == externalId &&
                item.EventType == eventType,
            cancellationToken);
    }

    public Task<IntegrationInbox?> GetInboxAsync(
        Guid organizationId,
        Guid inboxId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationInbox> query = dbContext.IntegrationInbox;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Id == inboxId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<IntegrationInbox>> ListInboxAsync(
        Guid organizationId,
        string? status,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationInbox> query = dbContext.IntegrationInbox
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = Enum.Parse<IntegrationInboxStatus>(
                status,
                ignoreCase: true);
            query = query.Where(item => item.Status == parsed);
        }

        return await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddInboxAsync(
        IntegrationInbox inbox,
        CancellationToken cancellationToken) =>
        await dbContext.IntegrationInbox.AddAsync(
            inbox,
            cancellationToken);

    public async Task AddEventAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        await dbContext.IntegrationEvents.AddAsync(
            integrationEvent,
            cancellationToken);

    public Task<IntegrationEvent?> GetEventAsync(
        Guid eventId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationEvent> query = dbContext.IntegrationEvents;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item => item.Id == eventId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookSubscription>> ListSubscriptionsAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<WebhookSubscription> query = dbContext
            .WebhookSubscriptions
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        return await query
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookSubscription>> ListMatchingSubscriptionsAsync(
        Guid organizationId,
        string eventType,
        CancellationToken cancellationToken)
    {
        var subscriptions = await dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organizationId &&
                item.IsActive)
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

        return subscriptions
            .Where(item => item.Matches(eventType))
            .ToArray();
    }

    public Task<WebhookSubscription?> GetSubscriptionAsync(
        Guid organizationId,
        Guid subscriptionId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<WebhookSubscription> query = dbContext.WebhookSubscriptions;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Id == subscriptionId,
            cancellationToken);
    }

    public async Task AddSubscriptionAsync(
        WebhookSubscription subscription,
        CancellationToken cancellationToken) =>
        await dbContext.WebhookSubscriptions.AddAsync(
            subscription,
            cancellationToken);

    public async Task AddDeliveryAsync(
        WebhookDelivery delivery,
        CancellationToken cancellationToken) =>
        await dbContext.WebhookDeliveries.AddAsync(
            delivery,
            cancellationToken);

    public Task<WebhookDelivery?> GetDeliveryAsync(
        Guid organizationId,
        Guid deliveryId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<WebhookDelivery> query = dbContext.WebhookDeliveries;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Id == deliveryId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> ListDeliveriesAsync(
        Guid organizationId,
        Guid? subscriptionId,
        string? status,
        CancellationToken cancellationToken)
    {
        IQueryable<WebhookDelivery> query = dbContext.WebhookDeliveries
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId);

        if (subscriptionId is not null)
        {
            query = query.Where(item =>
                item.WebhookSubscriptionId == subscriptionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = Enum.Parse<WebhookDeliveryStatus>(
                status,
                ignoreCase: true);
            query = query.Where(item => item.Status == parsed);
        }

        return await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookDelivery>> ListDueDeliveriesAsync(
        DateTimeOffset now,
        int take,
        CancellationToken cancellationToken) =>
        await dbContext.WebhookDeliveries
            .AsNoTracking()
            .Where(item =>
                item.Status == WebhookDeliveryStatus.Pending &&
                item.NextAttemptAtUtc <= now)
            .OrderBy(item => item.NextAttemptAtUtc)
            .ThenBy(item => item.CreatedAtUtc)
            .Take(take)
            .ToArrayAsync(cancellationToken);

    public Task<IntegrationFailure?> GetOpenFailureAsync(
        Guid organizationId,
        IntegrationFailureKind kind,
        Guid referenceId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationFailure> query = dbContext.IntegrationFailures;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Kind == kind &&
                item.ReferenceId == referenceId &&
                !item.IsResolved,
            cancellationToken);
    }

    public Task<IntegrationFailure?> GetFailureAsync(
        Guid organizationId,
        Guid failureId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationFailure> query = dbContext.IntegrationFailures;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(
            item =>
                item.OrganizationId == organizationId &&
                item.Id == failureId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<IntegrationFailure>> ListFailuresAsync(
        Guid organizationId,
        bool includeResolved,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationFailure> query = dbContext.IntegrationFailures
            .AsNoTracking()
            .Where(item => item.OrganizationId == organizationId);

        if (!includeResolved)
        {
            query = query.Where(item => !item.IsResolved);
        }

        return await query
            .OrderByDescending(item => item.LastAttemptAtUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddFailureAsync(
        IntegrationFailure failure,
        CancellationToken cancellationToken) =>
        await dbContext.IntegrationFailures.AddAsync(
            failure,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
