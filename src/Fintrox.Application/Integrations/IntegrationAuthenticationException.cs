namespace Fintrox.Application.Integrations;

public sealed class IntegrationAuthenticationException(string message)
    : Exception(message);
