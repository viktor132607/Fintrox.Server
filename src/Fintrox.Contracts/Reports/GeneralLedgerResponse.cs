namespace Fintrox.Contracts.Reports;

public sealed record GeneralLedgerResponse(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    DateOnly FromDate,
    DateOnly ToDate,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal PeriodDebit,
    decimal PeriodCredit,
    decimal ClosingDebit,
    decimal ClosingCredit,
    IReadOnlyList<GeneralLedgerLineResponse> Lines);
