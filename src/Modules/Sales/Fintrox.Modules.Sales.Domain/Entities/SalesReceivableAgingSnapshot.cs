namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesReceivableAgingSnapshot model; no cross-module navigations.</summary>
public sealed class SalesReceivableAgingSnapshot : OrganizationEntity
{
    public SalesReceivableAgingSnapshot(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        DateOnly asOfDate,
        decimal currentBalance,
        decimal overdueBalance,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        AsOfDate = DomainRules.Date(asOfDate, nameof(asOfDate));
        CurrentBalance = DomainRules.Nonnegative(currentBalance, nameof(currentBalance));
        OverdueBalance = DomainRules.Nonnegative(overdueBalance, nameof(overdueBalance));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid CounterpartyId { get; }
    public DateOnly AsOfDate { get; }
    public decimal CurrentBalance { get; }
    public decimal OverdueBalance { get; }
    public string Currency { get; }
}
