namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesInvoiceSchedule model; no cross-module navigations.</summary>
public sealed class SalesInvoiceSchedule : OrganizationEntity
{
    public SalesInvoiceSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        int scheduleNumber,
        DateOnly billingOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        ScheduleNumber = DomainRules.PositiveInt(scheduleNumber, nameof(scheduleNumber));
        BillingOn = DomainRules.Date(billingOn, nameof(billingOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesOrderId { get; }
    public int ScheduleNumber { get; }
    public DateOnly BillingOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
