namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned StorefrontChannel; external references validated by application layer.</summary>
public sealed class StorefrontChannel : OrganizationEntity
{
    public StorefrontChannel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storefrontId,
        string channelType,
        string externalStoreId,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        StorefrontId = DomainRules.Id(storefrontId, nameof(storefrontId));
        ChannelType = DomainRules.Text(channelType, nameof(channelType));
        ExternalStoreId = DomainRules.Text(externalStoreId, nameof(externalStoreId));
        IsEnabled = isEnabled;
    }

    public Guid StorefrontId { get; }
    public string ChannelType { get; }
    public string ExternalStoreId { get; }
    public bool IsEnabled { get; }
}
