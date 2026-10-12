namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesInvoiceAdjustment model; no cross-module navigations.</summary>
public sealed class SalesInvoiceAdjustment : OrganizationEntity
{
    public SalesInvoiceAdjustment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid invoiceId,
        string reason,
        decimal adjustmentAmount,
        string currency,
        DateOnly adjustedOn)
        : base(id, organizationId, createdAtUtc)
    {
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        Reason = DomainRules.Text(reason, nameof(reason));
        AdjustmentAmount = DomainRules.Nonzero(adjustmentAmount, nameof(adjustmentAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        AdjustedOn = DomainRules.Date(adjustedOn, nameof(adjustedOn));
    }

    public Guid InvoiceId { get; }
    public string Reason { get; }
    public decimal AdjustmentAmount { get; }
    public string Currency { get; }
    public DateOnly AdjustedOn { get; }
}
