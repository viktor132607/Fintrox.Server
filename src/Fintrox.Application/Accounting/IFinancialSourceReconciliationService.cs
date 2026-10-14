using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IFinancialSourceReconciliationService
{
    Task<FinancialSourceReconciliationResponse?> ReconcileAsync(
        Guid fiscalYearId, CancellationToken cancellationToken);
}
