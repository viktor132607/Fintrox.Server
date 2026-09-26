namespace Fintrox.Application.Integrations;

public interface IIntegrationBusinessEventHandler
{
    string Operation { get; }

    Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken);
}
