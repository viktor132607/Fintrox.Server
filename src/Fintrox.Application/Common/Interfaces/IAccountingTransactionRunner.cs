namespace Fintrox.Application.Common.Interfaces;

public interface IAccountingTransactionRunner
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);
}
