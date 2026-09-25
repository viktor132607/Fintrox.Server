using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Reports;

public sealed record GeneralLedgerMovement(
    Guid JournalEntryId,
    string JournalNumber,
    DateOnly PostingDate,
    DateOnly DocumentDate,
    string JournalDescription,
    int LineNumber,
    string? LineDescription,
    JournalEntrySource Source,
    string? ExternalReference,
    decimal Debit,
    decimal Credit);
