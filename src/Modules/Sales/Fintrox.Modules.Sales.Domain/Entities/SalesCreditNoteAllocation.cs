namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesCreditNoteAllocation model; no cross-module navigations.</summary>
public sealed class SalesCreditNoteAllocation : OrganizationEntity
{
    public SalesCreditNoteAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesCreditNoteId,
        Guid invoiceId,
        decimal appliedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SalesCreditNoteId = DomainRules.Id(salesCreditNoteId, nameof(salesCreditNoteId));
        InvoiceId = DomainRules.Id(invoiceId, nameof(invoiceId));
        AppliedAmount = DomainRules.Positive(appliedAmount, nameof(appliedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SalesCreditNoteId { get; }
    public Guid InvoiceId { get; }
    public decimal AppliedAmount { get; }
    public string Currency { get; }
}
