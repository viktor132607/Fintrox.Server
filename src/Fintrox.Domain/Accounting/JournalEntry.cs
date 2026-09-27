using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class JournalEntry : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private JournalEntry()
    {
    }

    private JournalEntry(
        Guid id,
        Guid organizationId,
        Guid fiscalPeriodId,
        DateOnly postingDate,
        DateOnly documentDate,
        string description,
        JournalEntrySource source,
        string? externalReference,
        Guid? reversalOfJournalEntryId,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        if (fiscalPeriodId == Guid.Empty)
        {
            throw new ArgumentException(
                "Fiscal period id is required.",
                nameof(fiscalPeriodId));
        }

        if (reversalOfJournalEntryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reversal source journal entry id cannot be empty.",
                nameof(reversalOfJournalEntryId));
        }

        FiscalPeriodId = fiscalPeriodId;
        PostingDate = postingDate;
        DocumentDate = documentDate;
        Description = NormalizeDescription(description);
        Source = source;
        ExternalReference = NormalizeExternalReference(externalReference);
        ReversalOfJournalEntryId = reversalOfJournalEntryId;
        Status = JournalEntryStatus.Draft;
    }

    public string? Number { get; private set; }

    public DateOnly PostingDate { get; private set; }

    public DateOnly DocumentDate { get; private set; }

    public string Description { get; private set; } = null!;

    public JournalEntryStatus Status { get; private set; }

    public JournalEntrySource Source { get; private set; }

    public string? ExternalReference { get; private set; }

    public Guid FiscalPeriodId { get; private set; }

    public DateTimeOffset? PostedAtUtc { get; private set; }

    public Guid? ReversalOfJournalEntryId { get; private set; }

    public Guid? ReversedByJournalEntryId { get; private set; }

    public static JournalEntry CreateDraft(
        Guid organizationId,
        Guid fiscalPeriodId,
        DateOnly postingDate,
        DateOnly documentDate,
        string description,
        JournalEntrySource source,
        string? externalReference,
        DateTimeOffset now)
    {
        return new JournalEntry(
            Guid.NewGuid(),
            organizationId,
            fiscalPeriodId,
            postingDate,
            documentDate,
            description,
            source,
            externalReference,
            reversalOfJournalEntryId: null,
            now);
    }

    public static JournalEntry CreateReversalDraft(
        Guid organizationId,
        Guid fiscalPeriodId,
        Guid reversalOfJournalEntryId,
        DateOnly postingDate,
        string description,
        DateTimeOffset now)
    {
        return new JournalEntry(
            Guid.NewGuid(),
            organizationId,
            fiscalPeriodId,
            postingDate,
            postingDate,
            description,
            JournalEntrySource.System,
            externalReference: null,
            reversalOfJournalEntryId,
            now);
    }

    public void UpdateDraft(
        Guid fiscalPeriodId,
        DateOnly postingDate,
        DateOnly documentDate,
        string description,
        string? externalReference,
        DateTimeOffset now)
    {
        EnsureDraft();

        if (ReversalOfJournalEntryId is not null)
        {
            throw new InvalidOperationException(
                "System-generated reversal drafts cannot be edited.");
        }

        if (fiscalPeriodId == Guid.Empty)
        {
            throw new ArgumentException(
                "Fiscal period id is required.",
                nameof(fiscalPeriodId));
        }

        FiscalPeriodId = fiscalPeriodId;
        PostingDate = postingDate;
        DocumentDate = documentDate;
        Description = NormalizeDescription(description);
        ExternalReference = NormalizeExternalReference(externalReference);
        Touch(now);
    }

    public void Post(string number, DateTimeOffset now)
    {
        EnsureDraft();

        var normalizedNumber = number?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedNumber))
        {
            throw new ArgumentException(
                "Journal entry number is required when posting.",
                nameof(number));
        }

        if (normalizedNumber.Length > 40)
        {
            throw new ArgumentException(
                "Journal entry number cannot exceed 40 characters.",
                nameof(number));
        }

        Number = normalizedNumber;
        PostedAtUtc = now;
        Status = JournalEntryStatus.Posted;
        Touch(now);
    }

    public void MarkReversed(
        Guid reversedByJournalEntryId,
        DateTimeOffset now)
    {
        if (Status != JournalEntryStatus.Posted)
        {
            throw new InvalidOperationException(
                "Only a posted journal entry can be reversed.");
        }

        if (ReversedByJournalEntryId is not null)
        {
            throw new InvalidOperationException(
                "This journal entry has already been reversed.");
        }

        if (reversedByJournalEntryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reversal journal entry id is required.",
                nameof(reversedByJournalEntryId));
        }

        ReversedByJournalEntryId = reversedByJournalEntryId;
        Status = JournalEntryStatus.Reversed;
        Touch(now);
    }

    public void EnsureDraft()
    {
        if (Status != JournalEntryStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft journal entries can be edited.");
        }
    }

    private static string NormalizeDescription(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "Journal entry description is required.",
                nameof(value));
        }

        if (normalized.Length > 500)
        {
            throw new ArgumentException(
                "Journal entry description cannot exceed 500 characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeExternalReference(string? value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > 160)
        {
            throw new ArgumentException(
                "External reference cannot exceed 160 characters.",
                nameof(value));
        }

        return normalized;
    }
}
