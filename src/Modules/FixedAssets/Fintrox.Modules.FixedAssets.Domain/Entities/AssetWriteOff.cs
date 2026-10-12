namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetWriteOff domain model.</summary>
public sealed class AssetWriteOff : OrganizationEntity
{
    public AssetWriteOff(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        DateOnly writtenOffOn,
        string reason,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        WrittenOffOn = DomainRules.Date(writtenOffOn, nameof(writtenOffOn));
        Reason = DomainRules.Text(reason, nameof(reason));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FixedAssetId { get; }
    public DateOnly WrittenOffOn { get; }
    public string Reason { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
