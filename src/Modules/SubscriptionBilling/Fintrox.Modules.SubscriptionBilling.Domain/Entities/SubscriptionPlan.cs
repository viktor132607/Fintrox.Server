namespace Fintrox.Modules.SubscriptionBilling.Domain.Entities;

public sealed class SubscriptionPlan : OrganizationEntity
{
    public SubscriptionPlan(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        int billingIntervalMonths,
        decimal unitPrice,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        BillingIntervalMonths = DomainRules.PositiveInt(billingIntervalMonths, nameof(billingIntervalMonths));
        UnitPrice = DomainRules.Nonnegative(unitPrice, nameof(unitPrice));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public string Code { get; }
    public string Name { get; }
    public int BillingIntervalMonths { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
}
