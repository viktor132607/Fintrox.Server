using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IProfitLossClosingService
{
    Task<FiscalCloseReconciliationResponse?> ReconcileAsync(
        Guid fiscalYearId,
        CancellationToken cancellationToken);

    Task<ProfitLossReopenResponse?> ReopenForCorrectionAsync(
        Guid fiscalYearId,
        Guid expectedClosingJournalEntryId,
        string reason,
        CancellationToken cancellationToken);

    Task<ProfitLossCloseResponse?> CloseAsync(
        Guid fiscalYearId,
        Guid retainedEarningsAccountId,
        CancellationToken cancellationToken);
}
