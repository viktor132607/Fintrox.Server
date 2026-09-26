using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Integrations;

public sealed record CreateWebhookSubscriptionRequest(
    [property: Required, MaxLength(160)] string Name,
    [property: Required, MaxLength(2048)] string TargetUrl,
    IReadOnlyCollection<string> EventTypes);
