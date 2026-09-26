namespace Fintrox.Application.Integrations;

public sealed class IntegrationBusinessEventDispatcher(
    IEnumerable<IIntegrationBusinessEventHandler> handlers)
    : IIntegrationBusinessEventDispatcher
{
    private readonly IReadOnlyDictionary<string, IIntegrationBusinessEventHandler> _handlers =
        handlers.ToDictionary(
            handler => handler.Operation,
            StringComparer.Ordinal);

    public Task<IntegrationBusinessEventResult> DispatchAsync(
        string operation,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var normalized = operation?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized) ||
            !_handlers.TryGetValue(normalized, out var handler))
        {
            throw new IntegrationProcessingException(
                $"Unsupported integration business operation '{operation}'.");
        }

        return handler.HandleAsync(
            payloadJson,
            cancellationToken);
    }
}
