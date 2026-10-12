namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCommissionAccrual model; no cross-module navigations.</summary>
public sealed class SalesCommissionAccrual : OrganizationEntity
{
    public SalesCommissionAccrual(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid salesPersonId,
        DateOnly accrualOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        SalesPersonId = DomainRules.Id(salesPersonId, nameof(salesPersonId));
        AccrualOn = DomainRules.Date(accrualOn, nameof(accrualOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesOrderId { get; }
    public Guid SalesPersonId { get; }
    public DateOnly AccrualOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
