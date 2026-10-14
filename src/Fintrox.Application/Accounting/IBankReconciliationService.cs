using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IBankReconciliationService
{
    Task<BankAccountResponse> CreateAccountAsync(
        CreateBankAccountRequest request, CancellationToken cancellationToken);

    Task<BankStatementImportResponse?> ImportStatementAsync(
        Guid accountId, ImportBankStatementRequest request,
        CancellationToken cancellationToken);

    Task<BankStatementMatchResponse?> MatchLineAsync(
        Guid statementId, Guid lineId, Guid paymentId,
        CancellationToken cancellationToken);

    Task<BankReconciliationResponse?> ReconcileAsync(
        Guid statementId, CancellationToken cancellationToken);
}
