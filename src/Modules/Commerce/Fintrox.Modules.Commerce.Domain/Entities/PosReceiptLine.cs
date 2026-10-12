namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PosReceiptLine; external references validated by application layer.</summary>
public sealed class PosReceiptLine : OrganizationEntity
{
    public PosReceiptLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid receiptId,
        Guid productVariantId,
        decimal quantity,
        decimal unitPrice,
        decimal taxAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ReceiptId = DomainRules.Id(receiptId, nameof(receiptId));
        ProductVariantId = DomainRules.Id(productVariantId, nameof(productVariantId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        TaxAmount = DomainRules.Nonnegative(taxAmount, nameof(taxAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ReceiptId { get; }
    public Guid ProductVariantId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal TaxAmount { get; }
    public string Currency { get; }
}
