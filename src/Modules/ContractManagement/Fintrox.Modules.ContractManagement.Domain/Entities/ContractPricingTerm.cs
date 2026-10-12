namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractPricingTerm; external references validated by application layer.</summary>
public sealed class ContractPricingTerm : OrganizationEntity
{
    public ContractPricingTerm(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid businessContractId,
        string priceComponent,
        decimal unitPrice,
        string currency,
        DateOnly effectiveFrom)
        : base(id, organizationId, createdAtUtc)
    {
        BusinessContractId = DomainRules.Id(businessContractId, nameof(businessContractId));
        PriceComponent = DomainRules.Text(priceComponent, nameof(priceComponent));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
    }

    public Guid BusinessContractId { get; }
    public string PriceComponent { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
    public DateOnly EffectiveFrom { get; }
}
