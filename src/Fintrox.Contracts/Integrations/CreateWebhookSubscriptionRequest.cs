using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Integrations;

public sealed record CreateWebhookSubscriptionRequest(
    [param: Required, MaxLength(160)] string Name,
    [param: Required, MaxLength(2048)] string TargetUrl,
    IReadOnlyCollection<string> EventTypes);
