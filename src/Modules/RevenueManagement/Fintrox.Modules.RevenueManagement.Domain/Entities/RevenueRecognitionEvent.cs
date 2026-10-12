namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionEvent model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionEvent : OrganizationEntity
{
    public RevenueRecognitionEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid scheduleLineId,
        Guid journalEntryId,
        DateOnly recognizedOn,
        decimal recognizedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ScheduleLineId = DomainRules.Id(scheduleLineId, nameof(scheduleLineId));
        JournalEntryId = DomainRules.Id(journalEntryId, nameof(journalEntryId));
        RecognizedOn = DomainRules.Date(recognizedOn, nameof(recognizedOn));
        RecognizedAmount = DomainRules.Nonnegative(recognizedAmount, nameof(recognizedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ScheduleLineId { get; }
    public Guid JournalEntryId { get; }
    public DateOnly RecognizedOn { get; }
    public decimal RecognizedAmount { get; }
    public string Currency { get; }
}
