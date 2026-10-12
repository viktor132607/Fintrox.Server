namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetComponent domain model.</summary>
public sealed class AssetComponent : OrganizationEntity
{
    public AssetComponent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        string componentCode,
        string description,
        decimal capitalizedCost,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        ComponentCode = DomainRules.Text(componentCode, nameof(componentCode));
        Description = DomainRules.Text(description, nameof(description));
        CapitalizedCost = DomainRules.Nonnegative(capitalizedCost, nameof(capitalizedCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public string ComponentCode { get; }
    public string Description { get; }
    public decimal CapitalizedCost { get; }
    public string Currency { get; }
}
