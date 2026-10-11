namespace Fintrox.Modules.Sales.Domain.Entities;

public sealed class SalesCreditNote : OrganizationEntity
{
    public SalesCreditNote(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid originalInvoiceId,
        string reference,
        decimal amount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        OriginalInvoiceId = DomainRules.Id(originalInvoiceId, nameof(originalInvoiceId));
        Reference = DomainRules.Text(reference, nameof(reference));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid OriginalInvoiceId { get; }
    public string Reference { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
