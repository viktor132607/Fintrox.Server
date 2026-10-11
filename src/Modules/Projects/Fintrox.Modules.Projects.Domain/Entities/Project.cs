namespace Fintrox.Modules.Projects.Domain.Entities;

public sealed class Project : OrganizationEntity
{
    public Project(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        DateOnly startsOn,
        DateOnly endsOn,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public string Code { get; }
    public string Name { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public string Currency { get; }
}
