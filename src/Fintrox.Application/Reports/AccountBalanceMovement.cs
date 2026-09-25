namespace Fintrox.Application.Reports;

public sealed record AccountBalanceMovement(
    Guid AccountId,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal PeriodDebit,
    decimal PeriodCredit);
