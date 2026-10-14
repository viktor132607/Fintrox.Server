using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IFinancialOpenItemReconciliationService
{
    Task<FinancialOpenItemReconciliationResponse?> ReconcileAsync(
        Guid fiscalYearId, CancellationToken cancellationToken);
}
