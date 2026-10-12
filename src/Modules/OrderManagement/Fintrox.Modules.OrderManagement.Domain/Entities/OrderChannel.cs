namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderChannel record.</summary>
public sealed class OrderChannel : OrganizationEntity
{
    public OrderChannel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string channelCode,
        string name,
        string sourceSystem,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ChannelCode = DomainRules.Text(channelCode, nameof(channelCode));
        Name = DomainRules.Text(name, nameof(name));
        SourceSystem = DomainRules.Text(sourceSystem, nameof(sourceSystem));
        IsActive = isActive;
    }

    public string ChannelCode { get; }
    public string Name { get; }
    public string SourceSystem { get; }
    public bool IsActive { get; }
}
