namespace Fintrox.Modules.Sales.Domain.Entities;

public sealed class SalesQuotationLine : OrganizationEntity
{
    public SalesQuotationLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid quotationId,
        Guid itemId,
        decimal quantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        QuotationId = DomainRules.Id(quotationId, nameof(quotationId));
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid QuotationId { get; }
    public Guid ItemId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
