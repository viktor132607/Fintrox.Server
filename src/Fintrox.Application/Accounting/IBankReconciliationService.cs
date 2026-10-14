using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IBankReconciliationService
{
    Task<IReadOnlyList<BankAccountResponse>> ListAccountsAsync(
        CancellationToken cancellationToken);

    Task<BankStatementResponse?> GetStatementAsync(
        Guid statementId, CancellationToken cancellationToken);

    Task<BankAccountResponse> CreateAccountAsync(
        CreateBankAccountRequest request, CancellationToken cancellationToken);

    Task<BankStatementImportResponse?> ImportStatementAsync(
        Guid accountId, ImportBankStatementRequest request,
        CancellationToken cancellationToken);

    Task<BankStatementImportResponse?> ImportCsvStatementAsync(
        Guid accountId, ImportCsvBankStatementRequest request,
        CancellationToken cancellationToken);

    Task<BankMatchSuggestionsResponse?> GetMatchSuggestionsAsync(
        Guid statementId, CancellationToken cancellationToken);

    Task<AutoMatchBankStatementResponse?> AutoMatchAsync(
        Guid statementId, CancellationToken cancellationToken);

    Task<ForeignBankSettlementResponse?> PostForeignSettlementAsync(
        Guid statementId, Guid lineId, PostForeignBankSettlementRequest request,
        CancellationToken cancellationToken);

    Task<BankFeePostingResponse?> PostFeeAsync(
        Guid statementId, Guid lineId, PostBankFeeRequest request,
        CancellationToken cancellationToken);

    Task<BankStatementMatchResponse?> MatchLineAsync(
        Guid statementId, Guid lineId, Guid paymentId,
        CancellationToken cancellationToken);

    Task<BankReconciliationResponse?> ReconcileAsync(
        Guid statementId, CancellationToken cancellationToken);
}
