namespace Fintrox.Modules.Purchases.Domain.Entities;

/// <summary>Immutable, organization-scoped PurchaseInvoiceMatchVariance model. References require application validation.</summary>
public sealed class PurchaseInvoiceMatchVariance : OrganizationEntity
{
    public PurchaseInvoiceMatchVariance(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid purchaseInvoiceMatchingId,
        string varianceType,
        decimal varianceAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PurchaseInvoiceMatchingId = DomainRules.Id(purchaseInvoiceMatchingId, nameof(purchaseInvoiceMatchingId));
        VarianceType = DomainRules.Text(varianceType, nameof(varianceType));
        VarianceAmount = DomainRules.Nonzero(varianceAmount, nameof(varianceAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PurchaseInvoiceMatchingId { get; }
    public string VarianceType { get; }
    public decimal VarianceAmount { get; }
    public string Currency { get; }
}
