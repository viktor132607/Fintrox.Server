namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccrualScheduleLine domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccrualScheduleLine : OrganizationEntity
{
    public AccrualScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid scheduleId,
        DateOnly periodDate,
        Guid journalEntryId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ScheduleId = DomainRules.Id(scheduleId, nameof(scheduleId));
        PeriodDate = DomainRules.Date(periodDate, nameof(periodDate));
        JournalEntryId = DomainRules.Id(journalEntryId, nameof(journalEntryId));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ScheduleId { get; }
    public DateOnly PeriodDate { get; }
    public Guid JournalEntryId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
