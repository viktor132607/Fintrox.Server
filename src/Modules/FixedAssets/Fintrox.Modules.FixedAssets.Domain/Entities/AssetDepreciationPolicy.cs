namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetDepreciationPolicy domain model.</summary>
public sealed class AssetDepreciationPolicy : OrganizationEntity
{
    public AssetDepreciationPolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        string method,
        int usefulLifeMonths,
        decimal residualValue,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        Method = DomainRules.Text(method, nameof(method));
        UsefulLifeMonths = DomainRules.PositiveInt(usefulLifeMonths, nameof(usefulLifeMonths));
        ResidualValue = DomainRules.Nonnegative(residualValue, nameof(residualValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public string Method { get; }
    public int UsefulLifeMonths { get; }
    public decimal ResidualValue { get; }
    public string Currency { get; }
}
