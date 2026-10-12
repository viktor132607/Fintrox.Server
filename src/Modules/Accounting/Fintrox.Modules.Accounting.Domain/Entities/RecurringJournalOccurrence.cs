namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped RecurringJournalOccurrence domain record; referenced objects are validated by application use cases.</summary>
public sealed class RecurringJournalOccurrence : OrganizationEntity
{
    public RecurringJournalOccurrence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid recurringJournalId,
        DateOnly dueOn,
        Guid generatedJournalId,
        DateTimeOffset processedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        RecurringJournalId = DomainRules.Id(recurringJournalId, nameof(recurringJournalId));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        GeneratedJournalId = DomainRules.Id(generatedJournalId, nameof(generatedJournalId));
        ProcessedAtUtc = DomainRules.Instant(processedAtUtc, nameof(processedAtUtc));
    }

    public Guid RecurringJournalId { get; }
    public DateOnly DueOn { get; }
    public Guid GeneratedJournalId { get; }
    public DateTimeOffset ProcessedAtUtc { get; }
}
