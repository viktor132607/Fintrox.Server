namespace Fintrox.Contracts.Reports;

public sealed record TrialBalanceResponse(
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalOpeningDebit,
    decimal TotalOpeningCredit,
    decimal TotalPeriodDebit,
    decimal TotalPeriodCredit,
    decimal TotalClosingDebit,
    decimal TotalClosingCredit,
    IReadOnlyList<TrialBalanceRowResponse> Rows);
