using Fintrox.Contracts.Reports;

namespace Fintrox.Application.Reports;

public interface IAccountingReportService
{
    Task<TrialBalanceResponse> GetTrialBalanceAsync(
        DateOnly? fromDate,
        DateOnly? toDate,
        bool includeZeroBalances,
        CancellationToken cancellationToken);

    Task<GeneralLedgerResponse?> GetGeneralLedgerAsync(
        Guid accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken);
}
