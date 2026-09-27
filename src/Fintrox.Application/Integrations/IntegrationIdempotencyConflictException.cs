namespace Fintrox.Application.Integrations;

public sealed class IntegrationIdempotencyConflictException(string message)
    : Exception(message);
