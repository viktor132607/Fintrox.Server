namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetMaintenanceEvent domain model.</summary>
public sealed class AssetMaintenanceEvent : OrganizationEntity
{
    public AssetMaintenanceEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        DateOnly serviceDate,
        string serviceType,
        decimal cost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        ServiceDate = DomainRules.Date(serviceDate, nameof(serviceDate));
        ServiceType = DomainRules.Text(serviceType, nameof(serviceType));
        Cost = DomainRules.Nonnegative(cost, nameof(cost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public DateOnly ServiceDate { get; }
    public string ServiceType { get; }
    public decimal Cost { get; }
    public string Currency { get; }
}
