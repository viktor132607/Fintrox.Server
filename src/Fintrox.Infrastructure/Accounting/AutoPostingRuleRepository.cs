using Fintrox.Application.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Accounting;

public sealed class AutoPostingRuleRepository(
    FintroxDbContext dbContext) : IAutoPostingRuleRepository
{
    public async Task<IReadOnlyList<AutoPostingRule>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        IQueryable<AutoPostingRule> query = dbContext.AutoPostingRules
            .AsNoTracking()
            .Where(rule => rule.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(rule => rule.IsActive);
        }

        return await query
            .OrderBy(rule => rule.Component)
            .ThenBy(rule => rule.MatchKind)
            .ThenBy(rule => rule.MatchValue)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutoPostingRule>> ListActiveAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.AutoPostingRules
            .AsNoTracking()
            .Where(rule =>
                rule.OrganizationId == organizationId &&
                rule.IsActive)
            .OrderBy(rule => rule.Component)
            .ThenBy(rule => rule.MatchKind)
            .ThenBy(rule => rule.MatchValue)
            .ToArrayAsync(cancellationToken);

    public async Task<AutoPostingRule?> GetAsync(
        Guid organizationId,
        Guid ruleId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<AutoPostingRule> query = dbContext.AutoPostingRules;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            rule =>
                rule.OrganizationId == organizationId &&
                rule.Id == ruleId,
            cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid organizationId,
        PostingComponent component,
        PostingRuleMatchKind matchKind,
        string matchValue,
        Guid? excludingRuleId,
        CancellationToken cancellationToken) =>
        dbContext.AutoPostingRules.AnyAsync(
            rule =>
                rule.OrganizationId == organizationId &&
                rule.Component == component &&
                rule.MatchKind == matchKind &&
                rule.MatchValue == matchValue &&
                (!excludingRuleId.HasValue ||
                 rule.Id != excludingRuleId.Value),
            cancellationToken);

    public async Task AddAsync(
        AutoPostingRule rule,
        CancellationToken cancellationToken) =>
        await dbContext.AutoPostingRules.AddAsync(
            rule,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
