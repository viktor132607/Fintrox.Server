namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRateBatchLine. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRateBatchLine : OrganizationEntity
{
    public CurrencyRateBatchLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid batchId,
        string quoteCurrency,
        decimal rate,
        string rateType)
        : base(id, organizationId, createdAtUtc)
    {
        BatchId = DomainRules.Id(batchId, nameof(batchId));
        QuoteCurrency = DomainRules.Currency(quoteCurrency, nameof(quoteCurrency));
        Rate = DomainRules.Positive(rate, nameof(rate));
        RateType = DomainRules.Text(rateType, nameof(rateType));
    }

    public Guid BatchId { get; }
    public string QuoteCurrency { get; }
    public decimal Rate { get; }
    public string RateType { get; }
}
