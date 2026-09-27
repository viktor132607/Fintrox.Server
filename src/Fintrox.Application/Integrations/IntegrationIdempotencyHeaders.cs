namespace Fintrox.Application.Integrations;

public static class IntegrationIdempotencyHeaders
{
    public const string SourceSystem = "X-Source-System";
    public const string ExternalId = "X-External-Id";
    public const string EventType = "X-Event-Type";
    public const string Replayed = "Idempotency-Replayed";
}
