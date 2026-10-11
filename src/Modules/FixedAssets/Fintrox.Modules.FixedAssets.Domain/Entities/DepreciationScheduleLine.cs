namespace Fintrox.Modules.FixedAssets.Domain.Entities;

public sealed class DepreciationScheduleLine : OrganizationEntity
{
    public DepreciationScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid assetId,
        DateOnly periodStartsOn,
        DateOnly periodEndsOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        AssetId = DomainRules.Id(assetId, nameof(assetId));
        PeriodStartsOn = DomainRules.Date(periodStartsOn, nameof(periodStartsOn));
        PeriodEndsOn = DomainRules.Date(periodEndsOn, nameof(periodEndsOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (PeriodEndsOn < PeriodStartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(periodEndsOn));
    }

    public Guid AssetId { get; }
    public DateOnly PeriodStartsOn { get; }
    public DateOnly PeriodEndsOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
