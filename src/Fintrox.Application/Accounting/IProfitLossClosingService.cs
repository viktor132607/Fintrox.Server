using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IProfitLossClosingService
{
    Task<ProfitLossCloseResponse?> CloseAsync(
        Guid fiscalYearId,
        Guid retainedEarningsAccountId,
        CancellationToken cancellationToken);
}
