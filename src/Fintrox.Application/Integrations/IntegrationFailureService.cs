using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class IntegrationFailureService(
    IIntegrationWorkflowRepository repository,
    IIntegrationInboxService inboxService,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IIntegrationFailureService
{
    public async Task<IReadOnlyList<IntegrationFailureResponse>> ListAsync(
        bool includeResolved,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var failures = await repository.ListFailuresAsync(
            organizationId,
            includeResolved,
            cancellationToken);

        return failures.Select(Map).ToArray();
    }

    public async Task<IntegrationFailureResponse?> GetAsync(
        Guid failureId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var failure = await repository.GetFailureAsync(
            organizationId,
            failureId,
            trackChanges: false,
            cancellationToken);

        return failure is null ? null : Map(failure);
    }

    public async Task<IntegrationFailureResponse?> RetryAsync(
        Guid failureId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var failure = await repository.GetFailureAsync(
            organizationId,
            failureId,
            trackChanges: true,
            cancellationToken);

        if (failure is null)
        {
            return null;
        }

        if (failure.IsResolved)
        {
            return Map(failure);
        }

        switch (failure.Kind)
        {
            case IntegrationFailureKind.InboxProcessing:
            {
                var referenceId = failure.ReferenceId;
                var inbox = await inboxService.RetryAsync(
                    referenceId,
                    cancellationToken);

                failure = await repository.GetFailureAsync(
                    organizationId,
                    failureId,
                    trackChanges: true,
                    cancellationToken)
                    ?? throw new InvalidOperationException(
                        "Integration failure disappeared during retry.");

                if (inbox is not null &&
                    string.Equals(
                        inbox.Status,
                        IntegrationInboxStatus.Succeeded.ToString(),
                        StringComparison.Ordinal))
                {
                    failure.Resolve(timeProvider.GetUtcNow());
                }

                break;
            }
            case IntegrationFailureKind.WebhookDelivery:
            {
                var delivery = await repository.GetDeliveryAsync(
                    organizationId,
                    failure.ReferenceId,
                    trackChanges: true,
                    cancellationToken);

                if (delivery is not null)
                {
                    delivery.Requeue(timeProvider.GetUtcNow());
                    failure.Resolve(timeProvider.GetUtcNow());
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(failure.Kind),
                    failure.Kind,
                    "Unsupported integration failure kind.");
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Map(failure);
    }

    private static IntegrationFailureResponse Map(
        IntegrationFailure failure) =>
        new(
            failure.Id,
            failure.OrganizationId,
            failure.Kind.ToString(),
            failure.ReferenceId,
            failure.Reason,
            failure.PayloadJson,
            failure.RetryCount,
            failure.LastAttemptAtUtc,
            failure.IsResolved,
            failure.ResolvedAtUtc,
            failure.CreatedAtUtc,
            failure.UpdatedAtUtc);
}
