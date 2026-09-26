using Fintrox.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fintrox.Infrastructure.Persistence;

public sealed class EfTransactionRunner(
    FintroxDbContext dbContext) : ITransactionRunner
{
    public Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var currentTransaction = dbContext.Database.CurrentTransaction;

        if (currentTransaction is not null)
        {
            return ExecuteWithSavepointAsync(
                currentTransaction,
                operation,
                cancellationToken);
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
                dbContext.ChangeTracker.Clear();
                throw;
            }
        });
    }

    private async Task<T> ExecuteWithSavepointAsync<T>(
        IDbContextTransaction transaction,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var savepointName =
            $"fintrox_{Guid.NewGuid():N}";

        await transaction.CreateSavepointAsync(
            savepointName,
            cancellationToken);

        try
        {
            var result = await operation(cancellationToken);

            await transaction.ReleaseSavepointAsync(
                savepointName,
                cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackToSavepointAsync(
                savepointName,
                cancellationToken);
            dbContext.ChangeTracker.Clear();

            try
            {
                await transaction.ReleaseSavepointAsync(
                    savepointName,
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // The outer transaction remains authoritative.
            }

            throw;
        }
    }
}
