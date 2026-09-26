namespace Fintrox.Contracts.Integrations;

public sealed record WebhookSubscriptionSecretResponse(
    WebhookSubscriptionResponse Subscription,
    string SigningSecret);
