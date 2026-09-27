using Fintrox.Domain.Accounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Accounting;

[TestClass]
public sealed class AutoPostingRuleTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid AccountId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateTimeOffset Now =
        new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void CreateDefaultRuleStoresWildcard()
    {
        var rule = AutoPostingRule.Create(
            OrganizationId,
            PostingComponent.AccountsReceivable,
            PostingRuleMatchKind.Default,
            null,
            AccountId,
            Now);

        Assert.AreEqual("*", rule.MatchValue);
        Assert.IsTrue(rule.IsActive);
    }

    [TestMethod]
    public void CreateSpecificRuleNormalizesMatchValue()
    {
        var rule = AutoPostingRule.Create(
            OrganizationId,
            PostingComponent.Revenue,
            PostingRuleMatchKind.ItemCode,
            " service-01 ",
            AccountId,
            Now);

        Assert.AreEqual("SERVICE-01", rule.MatchValue);
    }

    [TestMethod]
    public void InvalidComponentMatchCombinationIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            AutoPostingRule.Create(
                OrganizationId,
                PostingComponent.AccountsReceivable,
                PostingRuleMatchKind.ItemCode,
                "X",
                AccountId,
                Now));
    }

    [TestMethod]
    public void NonDefaultRuleRequiresMatchValue()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            AutoPostingRule.Create(
                OrganizationId,
                PostingComponent.OutputVat,
                PostingRuleMatchKind.VatCode,
                " ",
                AccountId,
                Now));
    }

    [TestMethod]
    public void RuleCanBeDeactivatedAndReactivated()
    {
        var rule = AutoPostingRule.Create(
            OrganizationId,
            PostingComponent.FxGain,
            PostingRuleMatchKind.Default,
            null,
            AccountId,
            Now);

        rule.Deactivate(Now.AddMinutes(1));
        Assert.IsFalse(rule.IsActive);

        rule.Activate(Now.AddMinutes(2));
        Assert.IsTrue(rule.IsActive);
    }
}
