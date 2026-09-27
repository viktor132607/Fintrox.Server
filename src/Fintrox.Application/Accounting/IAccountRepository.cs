using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<Account?> GetAsync(
        Guid organizationId,
        Guid accountId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, Account>> ListByIdsAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingAccountId,
        CancellationToken cancellationToken);

    Task<bool> HasActiveChildrenAsync(
        Guid organizationId,
        Guid accountId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Account account,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
