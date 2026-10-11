namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class Opportunity : OrganizationEntity
{
    public Opportunity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string name,
        decimal expectedValue,
        string currency,
        DateOnly expectedCloseOn)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Name = DomainRules.Text(name, nameof(name));
        ExpectedValue = DomainRules.Nonnegative(expectedValue, nameof(expectedValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ExpectedCloseOn = DomainRules.Date(expectedCloseOn, nameof(expectedCloseOn));
    }

    public Guid CounterpartyId { get; }
    public string Name { get; }
    public decimal ExpectedValue { get; }
    public string Currency { get; }
    public DateOnly ExpectedCloseOn { get; }
}
