namespace Fintrox.Application.Integrations;

public interface IIntegrationBusinessEventDispatcher
{
    Task<IntegrationBusinessEventResult> DispatchAsync(
        string operation,
        string payloadJson,
        CancellationToken cancellationToken);
}
