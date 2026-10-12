namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned OpportunityProduct record. Domain invariants enforced at construction.</summary>
public sealed class OpportunityProduct : OrganizationEntity
{
    public OpportunityProduct(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid opportunityId,
        Guid productId,
        decimal quantity,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        OpportunityId = DomainRules.Id(opportunityId, nameof(opportunityId));
        ProductId = DomainRules.Id(productId, nameof(productId));
        Quantity = DomainRules.Positive(quantity, nameof(quantity));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid OpportunityId { get; }
    public Guid ProductId { get; }
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
