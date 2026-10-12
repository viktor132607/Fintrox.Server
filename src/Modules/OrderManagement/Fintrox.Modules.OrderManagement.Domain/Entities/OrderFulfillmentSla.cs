namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderFulfillmentSla record.</summary>
public sealed class OrderFulfillmentSla : OrganizationEntity
{
    public OrderFulfillmentSla(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid channelId,
        string serviceLevelCode,
        decimal targetHours,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ChannelId = DomainRules.Id(channelId, nameof(channelId));
        ServiceLevelCode = DomainRules.Text(serviceLevelCode, nameof(serviceLevelCode));
        TargetHours = DomainRules.Positive(targetHours, nameof(targetHours));
        IsActive = isActive;
    }

    public Guid ChannelId { get; }
    public string ServiceLevelCode { get; }
    public decimal TargetHours { get; }
    public bool IsActive { get; }
}
