namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccrualSchedule domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccrualSchedule : OrganizationEntity
{
    public AccrualSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid accountId,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public string Name { get; }
    public Guid AccountId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
