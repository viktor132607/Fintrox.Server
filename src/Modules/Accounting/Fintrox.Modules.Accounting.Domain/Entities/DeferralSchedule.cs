namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped DeferralSchedule domain record; referenced objects are validated by application use cases.</summary>
public sealed class DeferralSchedule : OrganizationEntity
{
    public DeferralSchedule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid sourceJournalId,
        DateOnly startsOn,
        DateOnly endsOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        SourceJournalId = DomainRules.Id(sourceJournalId, nameof(sourceJournalId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public string Name { get; }
    public Guid SourceJournalId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
