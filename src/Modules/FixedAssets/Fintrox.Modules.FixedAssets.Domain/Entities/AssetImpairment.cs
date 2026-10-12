namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetImpairment domain model.</summary>
public sealed class AssetImpairment : OrganizationEntity
{
    public AssetImpairment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        DateOnly recognizedOn,
        decimal impairmentAmount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        RecognizedOn = DomainRules.Date(recognizedOn, nameof(recognizedOn));
        ImpairmentAmount = DomainRules.Nonnegative(impairmentAmount, nameof(impairmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid FixedAssetId { get; }
    public DateOnly RecognizedOn { get; }
    public decimal ImpairmentAmount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
