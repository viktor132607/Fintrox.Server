using Fintrox.Domain.Accounting;

namespace Fintrox.Application.Accounting;

public interface IAutoPostingRuleRepository
{
    Task<IReadOnlyList<AutoPostingRule>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AutoPostingRule>> ListActiveAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<AutoPostingRule?> GetAsync(
        Guid organizationId,
        Guid ruleId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid organizationId,
        PostingComponent component,
        PostingRuleMatchKind matchKind,
        string matchValue,
        Guid? excludingRuleId,
        CancellationToken cancellationToken);

    Task AddAsync(
        AutoPostingRule rule,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
