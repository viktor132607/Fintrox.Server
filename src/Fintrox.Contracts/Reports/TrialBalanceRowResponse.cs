namespace Fintrox.Contracts.Reports;

public sealed record TrialBalanceRowResponse(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    bool IsActive,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal PeriodDebit,
    decimal PeriodCredit,
    decimal ClosingDebit,
    decimal ClosingCredit);
