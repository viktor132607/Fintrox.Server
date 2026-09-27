using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

public sealed class AccountRepository(FintroxDbContext dbContext) : IAccountRepository
{
    public async Task<IReadOnlyList<Account>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<Account> query = dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(account => account.IsActive);
        }

        return await query
            .OrderBy(account => account.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Account?> GetAsync(
        Guid organizationId,
        Guid accountId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Account> query = dbContext.Accounts;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            account =>
                account.OrganizationId == organizationId &&
                account.Id == accountId,
            cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingAccountId,
        CancellationToken cancellationToken)
    {
        return dbContext.Accounts.AnyAsync(
            account =>
                account.OrganizationId == organizationId &&
                account.Code == code &&
                (!excludingAccountId.HasValue ||
                 account.Id != excludingAccountId.Value),
            cancellationToken);
    }

    public Task<bool> HasActiveChildrenAsync(
        Guid organizationId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        return dbContext.Accounts.AnyAsync(
            account =>
                account.OrganizationId == organizationId &&
                account.ParentAccountId == accountId &&
                account.IsActive,
            cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken)
    {
        await dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
