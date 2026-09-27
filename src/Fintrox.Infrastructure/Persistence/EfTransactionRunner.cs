using Fintrox.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Persistence;

public sealed class EfTransactionRunner(
    FintroxDbContext dbContext) : ITransactionRunner
{
    public Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (dbContext.Database.CurrentTransaction is not null)
        {
            return operation(cancellationToken);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();

            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(
                    cancellationToken);

            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
