namespace Fintrox.Application.Integrations;

public interface IIntegrationIdempotencyExecutor
{
    Task<IntegrationIdempotencyOutcome> ExecuteAsync(
        IntegrationIdempotencyRequest request,
        Func<CancellationToken, Task<IntegrationIdempotencyExecutionResult>> operation,
        CancellationToken cancellationToken);
}
