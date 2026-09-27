using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IAutoPostingRuleService
{
    Task<IReadOnlyList<AutoPostingRuleResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken);

    Task<AutoPostingRuleResponse?> GetAsync(
        Guid ruleId,
        CancellationToken cancellationToken);

    Task<AutoPostingRuleResponse> CreateAsync(
        CreateAutoPostingRuleRequest request,
        CancellationToken cancellationToken);

    Task<AutoPostingRuleResponse?> UpdateAsync(
        Guid ruleId,
        UpdateAutoPostingRuleRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid ruleId,
        CancellationToken cancellationToken);

    Task<AutoPostingRuleResponse?> ActivateAsync(
        Guid ruleId,
        CancellationToken cancellationToken);
}
