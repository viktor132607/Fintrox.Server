using Fintrox.Application.Integrations;
using Fintrox.Domain.Integrations;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Integrations;

public sealed class EfIntegrationIdempotencyExecutor(
    FintroxDbContext dbContext,
    TimeProvider timeProvider) : IIntegrationIdempotencyExecutor
{
    public async Task<IntegrationIdempotencyOutcome> ExecuteAsync(
        IntegrationIdempotencyRequest request,
        Func<CancellationToken, Task<IntegrationIdempotencyExecutionResult>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        dbContext.ChangeTracker.Clear();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var normalizedSourceSystem = request.SourceSystem
                .Trim()
                .ToLowerInvariant();
            var normalizedExternalId = request.ExternalId.Trim();
            var normalizedEventType = request.EventType
                .Trim()
                .ToLowerInvariant();

            var lockKey = string.Join(
                '|',
                request.OrganizationId.ToString("N"),
                normalizedSourceSystem,
                normalizedExternalId,
                normalizedEventType);

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0));",
                cancellationToken);

            var existing = await dbContext.IntegrationRequests
                .SingleOrDefaultAsync(
                    item =>
                        item.OrganizationId == request.OrganizationId &&
                        item.SourceSystem == normalizedSourceSystem &&
                        item.ExternalId == normalizedExternalId &&
                        item.EventType == normalizedEventType,
                    cancellationToken);

            if (existing is not null)
            {
                if (!string.Equals(
                        existing.RequestHash,
                        request.RequestHash,
                        StringComparison.Ordinal))
                {
                    throw new IntegrationIdempotencyConflictException(
                        "This external event key was already used with a different request payload or route.");
                }

                if (existing.Status != IntegrationRequestStatus.Completed ||
                    existing.ResponseStatusCode is null)
                {
                    throw new IntegrationIdempotencyConflictException(
                        "This external event is already being processed.");
                }

                await transaction.CommitAsync(cancellationToken);

                return new IntegrationIdempotencyOutcome(
                    Replayed: true,
                    existing.ResponseStatusCode.Value,
                    existing.ResponseContentType,
                    existing.ResponseBody,
                    existing.ResourceReference);
            }

            var record = IntegrationRequestRecord.Create(
                request.OrganizationId,
                request.IntegrationClientId,
                normalizedSourceSystem,
                normalizedExternalId,
                normalizedEventType,
                request.RequestMethod,
                request.RequestPath,
                request.RequestHash,
                timeProvider.GetUtcNow());

            await dbContext.IntegrationRequests.AddAsync(
                record,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await operation(cancellationToken);

            if (result.StatusCode >= 500)
            {
                await transaction.RollbackAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();

                return new IntegrationIdempotencyOutcome(
                    Replayed: false,
                    result.StatusCode,
                    result.ContentType,
                    result.Body,
                    result.ResourceReference);
            }

            record.Complete(
                result.StatusCode,
                result.ContentType,
                result.Body,
                result.ResourceReference,
                timeProvider.GetUtcNow());

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new IntegrationIdempotencyOutcome(
                Replayed: false,
                result.StatusCode,
                result.ContentType,
                result.Body,
                result.ResourceReference);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
