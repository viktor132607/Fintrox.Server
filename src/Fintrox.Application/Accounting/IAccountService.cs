using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccountTreeNodeResponse>> GetTreeAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<AccountResponse?> GetAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<AccountResponse> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken);

    Task<AccountResponse?> UpdateAsync(
        Guid accountId,
        UpdateAccountRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<AccountResponse?> ActivateAsync(
        Guid accountId,
        CancellationToken cancellationToken);
}
