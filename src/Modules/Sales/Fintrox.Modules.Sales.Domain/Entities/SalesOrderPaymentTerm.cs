namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderPaymentTerm model; no cross-module navigations.</summary>
public sealed class SalesOrderPaymentTerm : OrganizationEntity
{
    public SalesOrderPaymentTerm(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        Guid paymentTermId,
        DateOnly dueOn,
        decimal installmentPercent)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        PaymentTermId = DomainRules.Id(paymentTermId, nameof(paymentTermId));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        InstallmentPercent = DomainRules.Percent(installmentPercent, nameof(installmentPercent));
    }

    public Guid SalesOrderId { get; }
    public Guid PaymentTermId { get; }
    public DateOnly DueOn { get; }
    public decimal InstallmentPercent { get; }
}
