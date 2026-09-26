namespace Fintrox.Domain.Integrations;

public enum IntegrationFailureKind
{
    InboxProcessing = 0,
    WebhookDelivery = 10
}
