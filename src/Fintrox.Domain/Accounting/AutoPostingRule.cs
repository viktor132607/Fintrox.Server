using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed class AutoPostingRule : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private AutoPostingRule()
    {
    }

    private AutoPostingRule(
        Guid id,
        Guid organizationId,
        PostingComponent component,
        PostingRuleMatchKind matchKind,
        string? matchValue,
        Guid accountId,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        ValidateAccount(accountId);
        ValidateCombination(component, matchKind);

        Component = component;
        MatchKind = matchKind;
        MatchValue = NormalizeMatchValue(matchKind, matchValue);
        AccountId = accountId;
        IsActive = true;
    }

    public PostingComponent Component { get; private set; }

    public PostingRuleMatchKind MatchKind { get; private set; }

    public string MatchValue { get; private set; } = null!;

    public Guid AccountId { get; private set; }

    public bool IsActive { get; private set; }

    public static AutoPostingRule Create(
        Guid organizationId,
        PostingComponent component,
        PostingRuleMatchKind matchKind,
        string? matchValue,
        Guid accountId,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            organizationId,
            component,
            matchKind,
            matchValue,
            accountId,
            now);

    public void Update(
        PostingComponent component,
        PostingRuleMatchKind matchKind,
        string? matchValue,
        Guid accountId,
        DateTimeOffset now)
    {
        ValidateAccount(accountId);
        ValidateCombination(component, matchKind);

        Component = component;
        MatchKind = matchKind;
        MatchValue = NormalizeMatchValue(matchKind, matchValue);
        AccountId = accountId;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    public static string NormalizeMatchValue(
        PostingRuleMatchKind matchKind,
        string? value)
    {
        if (matchKind == PostingRuleMatchKind.Default)
        {
            return "*";
        }

        var normalized = value?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                "A match value is required for non-default posting rules.",
                nameof(value));
        }

        if (normalized.Length > 100)
        {
            throw new ArgumentException(
                "Posting rule match value cannot exceed 100 characters.",
                nameof(value));
        }

        return normalized;
    }

    public static void ValidateCombination(
        PostingComponent component,
        PostingRuleMatchKind matchKind)
    {
        var valid = component switch
        {
            PostingComponent.AccountsReceivable or
            PostingComponent.AccountsPayable or
            PostingComponent.CustomerAdvance or
            PostingComponent.SupplierAdvance or
            PostingComponent.FxGain or
            PostingComponent.FxLoss =>
                matchKind == PostingRuleMatchKind.Default,

            PostingComponent.Revenue or
            PostingComponent.Expense =>
                matchKind is PostingRuleMatchKind.Default
                    or PostingRuleMatchKind.ItemCode
                    or PostingRuleMatchKind.ProductCategory,

            PostingComponent.OutputVat or
            PostingComponent.InputVat =>
                matchKind is PostingRuleMatchKind.Default
                    or PostingRuleMatchKind.VatCode,

            PostingComponent.PaymentAsset =>
                matchKind is PostingRuleMatchKind.Default
                    or PostingRuleMatchKind.PaymentMethod,

            _ => false
        };

        if (!valid)
        {
            throw new ArgumentException(
                $"Match kind '{matchKind}' is not valid for posting component '{component}'.");
        }
    }

    private static void ValidateAccount(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "Posting rule account id is required.",
                nameof(accountId));
        }
    }
}
