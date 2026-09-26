namespace Fintrox.Application.Integrations;

public sealed class IntegrationProcessingException(string message)
    : Exception(message);
