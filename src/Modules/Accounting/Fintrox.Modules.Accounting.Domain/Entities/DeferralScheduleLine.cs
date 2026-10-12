namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped DeferralScheduleLine domain record; referenced objects are validated by application use cases.</summary>
public sealed class DeferralScheduleLine : OrganizationEntity
{
    public DeferralScheduleLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid scheduleId,
        DateOnly recognitionOn,
        decimal amount,
        string currency,
        Guid postedJournalId)
        : base(id, organizationId, createdAtUtc)
    {
        ScheduleId = DomainRules.Id(scheduleId, nameof(scheduleId));
        RecognitionOn = DomainRules.Date(recognitionOn, nameof(recognitionOn));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        PostedJournalId = DomainRules.Id(postedJournalId, nameof(postedJournalId));
    }

    public Guid ScheduleId { get; }
    public DateOnly RecognitionOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public Guid PostedJournalId { get; }
}
