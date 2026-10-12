namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped SupplierBid model. References require application validation.</summary>
public sealed class SupplierBid : OrganizationEntity
{
    public SupplierBid(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid quotationRequestId,
        Guid supplierId,
        DateOnly receivedOn,
        decimal totalPrice,
        string currency,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        QuotationRequestId = DomainRules.Id(quotationRequestId, nameof(quotationRequestId));
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        ReceivedOn = DomainRules.Date(receivedOn, nameof(receivedOn));
        TotalPrice = DomainRules.Nonnegative(totalPrice, nameof(totalPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid QuotationRequestId { get; }
    public Guid SupplierId { get; }
    public DateOnly ReceivedOn { get; }
    public decimal TotalPrice { get; }
    public string Currency { get; }
    public string Status { get; }
}
