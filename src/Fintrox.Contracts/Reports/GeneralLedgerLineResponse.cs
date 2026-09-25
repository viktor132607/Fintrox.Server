namespace Fintrox.Contracts.Reports;

public sealed record GeneralLedgerLineResponse(
    Guid JournalEntryId,
    string JournalNumber,
    DateOnly PostingDate,
    DateOnly DocumentDate,
    string JournalDescription,
    int LineNumber,
    string? LineDescription,
    string Source,
    string? ExternalReference,
    decimal Debit,
    decimal Credit,
    decimal RunningDebitBalance,
    decimal RunningCreditBalance);
