namespace Fintrox.Application.Accounting;

/// <summary>Immutable posted/possibly reversed source journal with its ledger lines.</summary>
public sealed record FinancialSourceJournalSnapshot(
    Guid Id, string ExternalReference, string Status,
    DateOnly PostingDate, Guid? ReversedByJournalEntryId,
    IReadOnlyList<FinancialSourceJournalLineSnapshot> Lines);

public sealed record FinancialSourceJournalLineSnapshot(
    int LineNumber, Guid AccountId,
    decimal Debit, decimal Credit, string? Description);

public sealed record FinancialPostedReversalSnapshot(
    Guid Id,
    Guid ReversalOfJournalEntryId,
    IReadOnlyList<FinancialSourceJournalLineSnapshot> Lines);
