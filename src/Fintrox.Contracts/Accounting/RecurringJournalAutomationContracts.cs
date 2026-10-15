namespace Fintrox.Contracts.Accounting;

/// <summary>Opt-in UTC-day draft generation only; never automatically posts GL.</summary>
public sealed record ConfigureRecurringJournalAutomationRequest(bool Enabled);

public sealed record RecurringJournalAutomationResponse(
    Guid TemplateId, bool Enabled, Guid ConfiguredByUserId,
    DateTimeOffset UpdatedAtUtc);

public sealed record RecurringJournalAutomationRunResponse(
    Guid RunId, Guid TemplateId, DateOnly PostingDate, string Status,
    int AttemptCount, Guid? JournalEntryId,
    DateTimeOffset? NextAttemptAtUtc, string? LastError);
