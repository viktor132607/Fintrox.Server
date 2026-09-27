using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public sealed class AccountService(
    IAccountRepository repository,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IAccountService
{
    public async Task<IReadOnlyList<AccountResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var accounts = await repository.ListAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        return accounts
            .OrderBy(account => account.Code, StringComparer.Ordinal)
            .Select(Map)
            .ToArray();
    }

    public async Task<IReadOnlyList<AccountTreeNodeResponse>> GetTreeAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var accounts = await repository.ListAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        var roots = accounts
            .Where(account => account.ParentAccountId is null)
            .OrderBy(account => account.Code, StringComparer.Ordinal)
            .ToArray();

        var childrenByParent = accounts
            .Where(account => account.ParentAccountId is not null)
            .GroupBy(account => account.ParentAccountId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(account => account.Code, StringComparer.Ordinal)
                    .ToArray());

        return roots
            .Select(account => BuildTreeNode(
                account,
                childrenByParent,
                new HashSet<Guid>()))
            .ToArray();
    }

    public async Task<AccountResponse?> GetAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var account = await repository.GetAsync(
            organizationId,
            accountId,
            trackChanges: false,
            cancellationToken);

        return account is null ? null : Map(account);
    }

    public async Task<AccountResponse> CreateAsync(
        CreateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var type = ParseType(request.Type);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (await repository.CodeExistsAsync(
                organizationId,
                normalizedCode,
                excludingAccountId: null,
                cancellationToken))
        {
            throw new AccountConflictException(
                $"Account code '{normalizedCode}' already exists in this organization.");
        }

        if (request.ParentAccountId is not null)
        {
            await ValidateParentAsync(
                organizationId,
                accountId: null,
                request.ParentAccountId.Value,
                type,
                cancellationToken);
        }

        var account = Account.Create(
            organizationId,
            normalizedCode,
            request.Name,
            type,
            request.ParentAccountId,
            request.IsAnalytical,
            request.AllowManualPosting,
            timeProvider.GetUtcNow());

        await repository.AddAsync(account, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(account);
    }

    public async Task<AccountResponse?> UpdateAsync(
        Guid accountId,
        UpdateAccountRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var account = await repository.GetAsync(
            organizationId,
            accountId,
            trackChanges: true,
            cancellationToken);

        if (account is null)
        {
            return null;
        }

        var type = ParseType(request.Type);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (await repository.CodeExistsAsync(
                organizationId,
                normalizedCode,
                accountId,
                cancellationToken))
        {
            throw new AccountConflictException(
                $"Account code '{normalizedCode}' already exists in this organization.");
        }

        if (request.ParentAccountId is not null)
        {
            await ValidateParentAsync(
                organizationId,
                accountId,
                request.ParentAccountId.Value,
                type,
                cancellationToken);
        }

        account.Update(
            normalizedCode,
            request.Name,
            type,
            request.ParentAccountId,
            request.IsAnalytical,
            request.AllowManualPosting,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task<bool> DeactivateAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var account = await repository.GetAsync(
            organizationId,
            accountId,
            trackChanges: true,
            cancellationToken);

        if (account is null)
        {
            return false;
        }

        if (await repository.HasActiveChildrenAsync(
                organizationId,
                accountId,
                cancellationToken))
        {
            throw new AccountConflictException(
                "An account with active child accounts cannot be deactivated.");
        }

        account.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AccountResponse?> ActivateAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var account = await repository.GetAsync(
            organizationId,
            accountId,
            trackChanges: true,
            cancellationToken);

        if (account is null)
        {
            return null;
        }

        if (account.ParentAccountId is not null)
        {
            var parent = await repository.GetAsync(
                organizationId,
                account.ParentAccountId.Value,
                trackChanges: false,
                cancellationToken);

            if (parent is null || !parent.IsActive)
            {
                throw new AccountConflictException(
                    "The parent account must be active before this account can be activated.");
            }
        }

        account.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return Map(account);
    }

    private async Task ValidateParentAsync(
        Guid organizationId,
        Guid? accountId,
        Guid parentAccountId,
        AccountType childType,
        CancellationToken cancellationToken)
    {
        if (accountId == parentAccountId)
        {
            throw new AccountConflictException(
                "An account cannot be its own parent.");
        }

        var parent = await repository.GetAsync(
            organizationId,
            parentAccountId,
            trackChanges: false,
            cancellationToken);

        if (parent is null)
        {
            throw new AccountConflictException(
                "The selected parent account does not exist in this organization.");
        }

        if (!parent.IsActive)
        {
            throw new AccountConflictException(
                "The selected parent account is inactive.");
        }

        if (parent.Type != childType)
        {
            throw new AccountConflictException(
                "A child account must have the same account type as its parent.");
        }

        if (accountId is null)
        {
            return;
        }

        var visited = new HashSet<Guid>();
        var current = parent;

        while (current.ParentAccountId is not null)
        {
            if (!visited.Add(current.Id))
            {
                throw new AccountConflictException(
                    "The account hierarchy contains a cycle.");
            }

            if (current.ParentAccountId == accountId)
            {
                throw new AccountConflictException(
                    "The selected parent would create an account hierarchy cycle.");
            }

            var next = await repository.GetAsync(
                organizationId,
                current.ParentAccountId.Value,
                trackChanges: false,
                cancellationToken);

            if (next is null)
            {
                break;
            }

            current = next;
        }
    }

    private static AccountType ParseType(string value)
    {
        if (!Enum.TryParse<AccountType>(
                value,
                ignoreCase: true,
                out var type) ||
            !Enum.IsDefined(type))
        {
            throw new AccountConflictException(
                $"Unknown account type '{value}'.");
        }

        return type;
    }

    private static AccountTreeNodeResponse BuildTreeNode(
        Account account,
        IReadOnlyDictionary<Guid, Account[]> childrenByParent,
        HashSet<Guid> ancestors)
    {
        if (!ancestors.Add(account.Id))
        {
            throw new AccountConflictException(
                "The account hierarchy contains a cycle.");
        }

        var nextAncestors = new HashSet<Guid>(ancestors);

        var children = childrenByParent.TryGetValue(account.Id, out var directChildren)
            ? directChildren
                .Select(child => BuildTreeNode(
                    child,
                    childrenByParent,
                    nextAncestors))
                .ToArray()
            : [];

        return new AccountTreeNodeResponse(
            Map(account),
            children);
    }

    private static AccountResponse Map(Account account)
    {
        return new AccountResponse(
            account.Id,
            account.OrganizationId,
            account.Code,
            account.Name,
            account.Type.ToString(),
            account.ParentAccountId,
            account.IsAnalytical,
            account.IsActive,
            account.AllowManualPosting,
            account.CreatedAtUtc,
            account.CreatedByUserId,
            account.UpdatedAtUtc,
            account.UpdatedByUserId);
    }
}
