namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierQuotationRequestLine model. References require application validation.</summary>
public sealed class SupplierQuotationRequestLine : OrganizationEntity
{
    public SupplierQuotationRequestLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid quotationRequestId,
        Guid inventoryItemId,
        decimal requestedQuantity,
        decimal targetPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        QuotationRequestId = DomainRules.Id(quotationRequestId, nameof(quotationRequestId));
        InventoryItemId = DomainRules.Id(inventoryItemId, nameof(inventoryItemId));
        RequestedQuantity = DomainRules.Positive(requestedQuantity, nameof(requestedQuantity));
        TargetPrice = DomainRules.Nonnegative(targetPrice, nameof(targetPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid QuotationRequestId { get; }
    public Guid InventoryItemId { get; }
    public decimal RequestedQuantity { get; }
    public decimal TargetPrice { get; }
    public string Currency { get; }
}
