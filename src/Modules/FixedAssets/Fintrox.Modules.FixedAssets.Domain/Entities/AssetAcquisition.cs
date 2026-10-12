namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetAcquisition domain model.</summary>
public sealed class AssetAcquisition : OrganizationEntity
{
    public AssetAcquisition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        Guid supplierId,
        DateOnly acquisitionOn,
        decimal acquisitionCost,
        string currency,
        Guid sourceDocumentId)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        AcquisitionOn = DomainRules.Date(acquisitionOn, nameof(acquisitionOn));
        AcquisitionCost = DomainRules.Nonnegative(acquisitionCost, nameof(acquisitionCost));
        Currency = DomainRules.Currency(currency, nameof(currency));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
    }

    public Guid FixedAssetId { get; }
    public Guid SupplierId { get; }
    public DateOnly AcquisitionOn { get; }
    public decimal AcquisitionCost { get; }
    public string Currency { get; }
    public Guid SourceDocumentId { get; }
}
