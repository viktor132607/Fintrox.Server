namespace Fintrox.Application.Common.Interfaces;

public interface ITransactionRunner
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);
}
