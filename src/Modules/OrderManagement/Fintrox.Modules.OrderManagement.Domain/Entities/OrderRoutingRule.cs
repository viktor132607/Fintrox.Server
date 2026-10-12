namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderRoutingRule record.</summary>
public sealed class OrderRoutingRule : OrganizationEntity
{
    public OrderRoutingRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid channelId,
        int priority,
        string conditionJson,
        Guid fulfillmentLocationId,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ChannelId = DomainRules.Id(channelId, nameof(channelId));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
        ConditionJson = DomainRules.Json(conditionJson, nameof(conditionJson));
        FulfillmentLocationId = DomainRules.Id(fulfillmentLocationId, nameof(fulfillmentLocationId));
        IsEnabled = isEnabled;
    }

    public Guid ChannelId { get; }
    public int Priority { get; }
    public string ConditionJson { get; }
    public Guid FulfillmentLocationId { get; }
    public bool IsEnabled { get; }
}
