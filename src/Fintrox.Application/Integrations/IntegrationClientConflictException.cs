namespace Fintrox.Application.Integrations;

public sealed class IntegrationClientConflictException(string message)
    : Exception(message);
