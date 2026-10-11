namespace Fintrox.Modules.Accounting.Domain.Entities;

public sealed class RecurringJournal : OrganizationEntity
{
    public RecurringJournal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        Guid sourceJournalId,
        DateOnly startsOn,
        DateOnly endsOn,
        int intervalMonths)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        SourceJournalId = DomainRules.Id(sourceJournalId, nameof(sourceJournalId));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        IntervalMonths = DomainRules.PositiveInt(intervalMonths, nameof(intervalMonths));
        if (EndsOn < StartsOn) throw new ArgumentException("The end cannot precede the start.", nameof(endsOn));
    }

    public string Name { get; }
    public Guid SourceJournalId { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public int IntervalMonths { get; }
}
