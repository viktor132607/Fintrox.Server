namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRevaluationLine. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRevaluationLine : OrganizationEntity
{
    public CurrencyRevaluationLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid revaluationId,
        Guid accountId,
        decimal originalAmount,
        decimal revaluedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        RevaluationId = DomainRules.Id(revaluationId, nameof(revaluationId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        OriginalAmount = DomainRules.Nonzero(originalAmount, nameof(originalAmount));
        RevaluedAmount = DomainRules.Nonzero(revaluedAmount, nameof(revaluedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid RevaluationId { get; }
    public Guid AccountId { get; }
    public decimal OriginalAmount { get; }
    public decimal RevaluedAmount { get; }
    public string Currency { get; }
}
