namespace Fintrox.Application.Integrations;

public interface IIntegrationBusinessEventProcessor
{
    string Operation { get; }

    Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken);
}
