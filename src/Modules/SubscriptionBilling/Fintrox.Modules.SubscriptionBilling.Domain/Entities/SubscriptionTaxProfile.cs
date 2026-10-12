namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

/// <summary>Tenant-scoped SubscriptionTaxProfile domain record. No cross-module navigation properties.</summary>
public sealed class SubscriptionTaxProfile : OrganizationEntity
{
    public SubscriptionTaxProfile(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid subscriptionId,
        string taxCode,
        string countryCode,
        bool isReverseCharge)
        : base(id, organizationId, createdAtUtc)
    {
        SubscriptionId = DomainRules.Id(subscriptionId, nameof(subscriptionId));
        TaxCode = DomainRules.Text(taxCode, nameof(taxCode));
        CountryCode = DomainRules.Country(countryCode, nameof(countryCode));
        IsReverseCharge = isReverseCharge;
    }

    public Guid SubscriptionId { get; }
    public string TaxCode { get; }
    public string CountryCode { get; }
    public bool IsReverseCharge { get; }
}
