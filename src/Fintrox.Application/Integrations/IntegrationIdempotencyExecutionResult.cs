namespace Fintrox.Application.Integrations;

public sealed record IntegrationIdempotencyExecutionResult(
    int StatusCode,
    string? ContentType,
    string? Body,
    string? ResourceReference);
