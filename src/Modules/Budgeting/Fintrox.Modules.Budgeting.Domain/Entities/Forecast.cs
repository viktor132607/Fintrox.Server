namespace Fintrox.Modules.Budgeting.Domain.Entities;

public sealed class Forecast : OrganizationEntity
{
    public Forecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        DateOnly startsOn,
        DateOnly endsOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public string Name { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Currency { get; }
}
