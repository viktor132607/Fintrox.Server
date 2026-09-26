namespace Fintrox.Application.Integrations;

public sealed record IntegrationIdempotencyOutcome(
    bool Replayed,
    int StatusCode,
    string? ContentType,
    string? Body,
    string? ResourceReference);
