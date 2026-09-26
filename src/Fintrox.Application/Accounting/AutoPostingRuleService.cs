using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Accounting;
using Fintrox.Domain.Accounting;
using Fintrox.Domain.Payments;

namespace Fintrox.Application.Accounting;

public sealed class AutoPostingRuleService(
    IAutoPostingRuleRepository repository,
    IAccountRepository accountRepository,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IAutoPostingRuleService
{
    public async Task<IReadOnlyList<AutoPostingRuleResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rules = await repository.ListAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        return await MapAsync(
            organizationId,
            rules,
            cancellationToken);
    }

    public async Task<AutoPostingRuleResponse?> GetAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rule = await repository.GetAsync(
            organizationId,
            ruleId,
            trackChanges: false,
            cancellationToken);

        if (rule is null)
        {
            return null;
        }

        return (await MapAsync(
            organizationId,
            [rule],
            cancellationToken))[0];
    }

    public async Task<AutoPostingRuleResponse> CreateAsync(
        CreateAutoPostingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var component = ParseComponent(request.Component);
        var matchKind = ParseMatchKind(request.MatchKind);
        var matchValue = NormalizeRequestMatchValue(
            matchKind,
            request.MatchValue);
        var normalizedMatch = AutoPostingRule.NormalizeMatchValue(
            matchKind,
            matchValue);

        AutoPostingRule.ValidateCombination(component, matchKind);

        await ValidateAccountAsync(
            organizationId,
            component,
            request.AccountId,
            cancellationToken);

        if (await repository.ExistsAsync(
                organizationId,
                component,
                matchKind,
                normalizedMatch,
                excludingRuleId: null,
                cancellationToken))
        {
            throw new AutoPostingRuleConflictException(
                "An auto-posting rule already exists for this component and match.");
        }

        var rule = AutoPostingRule.Create(
            organizationId,
            component,
            matchKind,
            matchValue,
            request.AccountId,
            timeProvider.GetUtcNow());

        await repository.AddAsync(rule, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return (await MapAsync(
            organizationId,
            [rule],
            cancellationToken))[0];
    }

    public async Task<AutoPostingRuleResponse?> UpdateAsync(
        Guid ruleId,
        UpdateAutoPostingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rule = await repository.GetAsync(
            organizationId,
            ruleId,
            trackChanges: true,
            cancellationToken);

        if (rule is null)
        {
            return null;
        }

        var component = ParseComponent(request.Component);
        var matchKind = ParseMatchKind(request.MatchKind);
        var matchValue = NormalizeRequestMatchValue(
            matchKind,
            request.MatchValue);
        var normalizedMatch = AutoPostingRule.NormalizeMatchValue(
            matchKind,
            matchValue);

        AutoPostingRule.ValidateCombination(component, matchKind);

        await ValidateAccountAsync(
            organizationId,
            component,
            request.AccountId,
            cancellationToken);

        if (await repository.ExistsAsync(
                organizationId,
                component,
                matchKind,
                normalizedMatch,
                rule.Id,
                cancellationToken))
        {
            throw new AutoPostingRuleConflictException(
                "An auto-posting rule already exists for this component and match.");
        }

        rule.Update(
            component,
            matchKind,
            matchValue,
            request.AccountId,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);

        return (await MapAsync(
            organizationId,
            [rule],
            cancellationToken))[0];
    }

    public async Task<bool> DeactivateAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rule = await repository.GetAsync(
            organizationId,
            ruleId,
            trackChanges: true,
            cancellationToken);

        if (rule is null)
        {
            return false;
        }

        rule.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AutoPostingRuleResponse?> ActivateAsync(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var rule = await repository.GetAsync(
            organizationId,
            ruleId,
            trackChanges: true,
            cancellationToken);

        if (rule is null)
        {
            return null;
        }

        await ValidateAccountAsync(
            organizationId,
            rule.Component,
            rule.AccountId,
            cancellationToken);

        rule.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return (await MapAsync(
            organizationId,
            [rule],
            cancellationToken))[0];
    }

    private async Task ValidateAccountAsync(
        Guid organizationId,
        PostingComponent component,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetAsync(
            organizationId,
            accountId,
            trackChanges: false,
            cancellationToken);

        if (account is null)
        {
            throw new AutoPostingRuleConflictException(
                "The selected account does not exist in this organization.");
        }

        if (!account.IsActive)
        {
            throw new AutoPostingRuleConflictException(
                $"Account '{account.Code}' is inactive.");
        }

        var expectedType = component switch
        {
            PostingComponent.AccountsReceivable or
            PostingComponent.InputVat or
            PostingComponent.PaymentAsset or
            PostingComponent.SupplierAdvance => AccountType.Asset,

            PostingComponent.AccountsPayable or
            PostingComponent.OutputVat or
            PostingComponent.CustomerAdvance => AccountType.Liability,

            PostingComponent.Revenue or
            PostingComponent.FxGain => AccountType.Revenue,

            PostingComponent.Expense or
            PostingComponent.FxLoss => AccountType.Expense,

            _ => throw new ArgumentOutOfRangeException(
                nameof(component),
                component,
                "Unsupported posting component.")
        };

        if (account.Type != expectedType)
        {
            throw new AutoPostingRuleConflictException(
                $"Posting component '{component}' requires an account of type '{expectedType}', but account '{account.Code}' is '{account.Type}'.");
        }
    }

    private async Task<IReadOnlyList<AutoPostingRuleResponse>> MapAsync(
        Guid organizationId,
        IReadOnlyList<AutoPostingRule> rules,
        CancellationToken cancellationToken)
    {
        var accounts = await accountRepository.ListByIdsAsync(
            organizationId,
            rules.Select(rule => rule.AccountId).Distinct().ToArray(),
            cancellationToken);

        return rules.Select(rule =>
        {
            if (!accounts.TryGetValue(rule.AccountId, out var account))
            {
                throw new InvalidOperationException(
                    "An auto-posting rule references a missing account.");
            }

            return new AutoPostingRuleResponse(
                rule.Id,
                rule.OrganizationId,
                rule.Component.ToString(),
                rule.MatchKind.ToString(),
                rule.MatchKind == PostingRuleMatchKind.Default
                    ? null
                    : rule.MatchValue,
                rule.AccountId,
                account.Code,
                account.Name,
                rule.IsActive,
                rule.CreatedAtUtc,
                rule.UpdatedAtUtc);
        }).ToArray();
    }

    private static PostingComponent ParseComponent(string value) =>
        Enum.TryParse<PostingComponent>(
            value?.Trim(),
            ignoreCase: true,
            out var parsed)
            ? parsed
            : throw new ArgumentException(
                $"Unknown posting component '{value}'.",
                nameof(value));

    private static PostingRuleMatchKind ParseMatchKind(string value) =>
        Enum.TryParse<PostingRuleMatchKind>(
            value?.Trim(),
            ignoreCase: true,
            out var parsed)
            ? parsed
            : throw new ArgumentException(
                $"Unknown posting rule match kind '{value}'.",
                nameof(value));

    private static string? NormalizeRequestMatchValue(
        PostingRuleMatchKind matchKind,
        string? value)
    {
        if (matchKind != PostingRuleMatchKind.PaymentMethod)
        {
            return value;
        }

        if (!Enum.TryParse<PaymentMethod>(
                value?.Trim(),
                ignoreCase: true,
                out var paymentMethod))
        {
            throw new ArgumentException(
                $"Unknown payment method '{value}'.",
                nameof(value));
        }

        return paymentMethod.ToString();
    }
}
