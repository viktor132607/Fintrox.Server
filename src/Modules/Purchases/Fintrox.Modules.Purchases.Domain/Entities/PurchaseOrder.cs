namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class PurchaseOrder : OrganizationEntity
{
    public PurchaseOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supplierId,
        string reference,
        DateOnly orderedOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        SupplierId = DomainRules.Id(supplierId, nameof(supplierId));
        Reference = DomainRules.Text(reference, nameof(reference));
        OrderedOn = DomainRules.Date(orderedOn, nameof(orderedOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid SupplierId { get; }
    public string Reference { get; }
    public DateOnly OrderedOn { get; }
    public string Currency { get; }
}
