namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetRevaluation domain model.</summary>
public sealed class AssetRevaluation : OrganizationEntity
{
    public AssetRevaluation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        DateOnly revaluedOn,
        decimal previousValue,
        decimal newValue,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        RevaluedOn = DomainRules.Date(revaluedOn, nameof(revaluedOn));
        PreviousValue = DomainRules.Nonnegative(previousValue, nameof(previousValue));
        NewValue = DomainRules.Nonnegative(newValue, nameof(newValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public DateOnly RevaluedOn { get; }
    public decimal PreviousValue { get; }
    public decimal NewValue { get; }
    public string Currency { get; }
}
