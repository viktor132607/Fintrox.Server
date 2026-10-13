using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

/// <summary>
/// Snapshot of a conditional database lease. Expired final attempts are
/// terminally recorded without performing a seventh HTTP send.
/// </summary>
public sealed record WebhookDeliveryClaim(
    WebhookDelivery Delivery,
    bool IsExhausted);
