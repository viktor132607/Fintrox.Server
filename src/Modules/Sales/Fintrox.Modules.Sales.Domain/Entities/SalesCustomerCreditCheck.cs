namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCustomerCreditCheck model; no cross-module navigations.</summary>
public sealed class SalesCustomerCreditCheck : OrganizationEntity
{
    public SalesCustomerCreditCheck(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid counterpartyId,
        DateTimeOffset checkedAtUtc,
        decimal availableCredit,
        string currency,
        string result)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        CheckedAtUtc = DomainRules.Instant(checkedAtUtc, nameof(checkedAtUtc));
        AvailableCredit = DomainRules.Nonnegative(availableCredit, nameof(availableCredit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Result = DomainRules.Text(result, nameof(result));
    }

    public Guid SalesOrderId { get; }
    public Guid CounterpartyId { get; }
    public DateTimeOffset CheckedAtUtc { get; }
    public decimal AvailableCredit { get; }
    public string Currency { get; }
    public string Result { get; }
}
