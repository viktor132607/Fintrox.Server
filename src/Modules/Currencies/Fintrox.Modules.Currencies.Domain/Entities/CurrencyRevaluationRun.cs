namespace Fintrox.Modules.Currencies.Domain.Entities;

/// <summary>Tenant-scoped CurrencyRevaluationRun. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CurrencyRevaluationRun : OrganizationEntity
{
    public CurrencyRevaluationRun(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingBookId,
        DateOnly asOfDate,
        string baseCurrency,
        string status,
        Guid executedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingBookId = DomainRules.Id(accountingBookId, nameof(accountingBookId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        BaseCurrency = DomainRules.Currency(baseCurrency, nameof(baseCurrency));
        Status = DomainRules.Text(status, nameof(status));
        ExecutedByUserId = DomainRules.Id(executedByUserId, nameof(executedByUserId));
    }

    public Guid AccountingBookId { get; }
    public DateOnly AsOfDate { get; }
    public string BaseCurrency { get; }
    public string Status { get; }
    public Guid ExecutedByUserId { get; }
}
