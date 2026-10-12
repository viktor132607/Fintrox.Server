namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetCapitalization domain model.</summary>
public sealed class AssetCapitalization : OrganizationEntity
{
    public AssetCapitalization(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        DateOnly capitalizedOn,
        Guid sourceDocumentId,
        decimal capitalizedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        CapitalizedOn = DomainRules.Date(capitalizedOn, nameof(capitalizedOn));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        CapitalizedAmount = DomainRules.Nonnegative(capitalizedAmount, nameof(capitalizedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public DateOnly CapitalizedOn { get; }
    public Guid SourceDocumentId { get; }
    public decimal CapitalizedAmount { get; }
    public string Currency { get; }
}
