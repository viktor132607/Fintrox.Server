namespace Fintrox.Modules.FixedAssets.Domain.Entities;

public sealed class AssetDisposal : OrganizationEntity
{
    public AssetDisposal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid assetId,
        DateOnly disposedOn,
        decimal proceeds,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        AssetId = DomainRules.Id(assetId, nameof(assetId));
        DisposedOn = DomainRules.Date(disposedOn, nameof(disposedOn));
        Proceeds = DomainRules.Nonnegative(proceeds, nameof(proceeds));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid AssetId { get; }
    public DateOnly DisposedOn { get; }
    public decimal Proceeds { get; }
    public string Currency { get; }
    public string Reason { get; }
}
