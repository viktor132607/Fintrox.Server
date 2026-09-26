using Fintrox.Domain.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Integrations;

[TestClass]
public sealed class IntegrationInboxOutboxTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly DateTimeOffset Now =
        new(2026, 9, 26, 18, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void InboxMovesFromReceivedToSucceeded()
    {
        var inbox = CreateInbox();

        inbox.StartProcessing(Now.AddSeconds(1));
        inbox.Succeed(
            "SalesInvoice",
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Now.AddSeconds(2));

        Assert.AreEqual(
            IntegrationInboxStatus.Succeeded,
            inbox.Status);
        Assert.AreEqual(
            "SalesInvoice",
            inbox.ResultEntityType);
        Assert.IsNotNull(inbox.CompletedAtUtc);
    }

    [TestMethod]
    public void InboxFailureTracksRetryCountAndReason()
    {
        var inbox = CreateInbox();

        inbox.StartProcessing(Now.AddSeconds(1));
        inbox.Fail(
            "Posting rules are incomplete.",
            Now.AddSeconds(2));

        Assert.AreEqual(
            IntegrationInboxStatus.Failed,
            inbox.Status);
        Assert.AreEqual(1, inbox.RetryCount);
        Assert.AreEqual(
            "Posting rules are incomplete.",
            inbox.FailureReason);
    }

    [TestMethod]
    public void WebhookSubscriptionMatchesExactAndWildcardEvents()
    {
        var exact = CreateSubscription(
            "sales.invoice.issued,payment.confirmed");
        var wildcard = CreateSubscription("*");

        Assert.IsTrue(
            exact.Matches("sales.invoice.issued"));
        Assert.IsFalse(
            exact.Matches("counterparty.created"));
        Assert.IsTrue(
            wildcard.Matches("counterparty.created"));
    }

    [TestMethod]
    public void WebhookDeliveryCanRetryThenSucceed()
    {
        var delivery = WebhookDelivery.Create(
            OrganizationId,
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Now);

        delivery.StartAttempt(Now.AddSeconds(1));
        delivery.ScheduleRetry(
            503,
            "Unavailable",
            Now.AddMinutes(1),
            Now.AddSeconds(2));

        Assert.AreEqual(1, delivery.AttemptCount);
        Assert.AreEqual(
            WebhookDeliveryStatus.Pending,
            delivery.Status);

        delivery.StartAttempt(Now.AddMinutes(1));
        delivery.Succeed(
            200,
            Now.AddMinutes(1).AddSeconds(1));

        Assert.AreEqual(
            WebhookDeliveryStatus.Delivered,
            delivery.Status);
        Assert.AreEqual(2, delivery.AttemptCount);
    }

    [TestMethod]
    public void IntegrationFailureCanBeResolved()
    {
        var failure = IntegrationFailure.Create(
            OrganizationId,
            IntegrationFailureKind.InboxProcessing,
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Failed",
            "{}",
            Now);

        failure.Resolve(Now.AddMinutes(1));

        Assert.IsTrue(failure.IsResolved);
        Assert.AreEqual(
            Now.AddMinutes(1),
            failure.ResolvedAtUtc);
    }

    private static IntegrationInbox CreateInbox() =>
        IntegrationInbox.Create(
            OrganizationId,
            "shop-api",
            "order-42",
            "order.completed",
            "sales",
            "{}",
            Now);

    private static WebhookSubscription CreateSubscription(
        string eventTypes) =>
        WebhookSubscription.Create(
            OrganizationId,
            "Demo",
            "https://example.com/webhooks",
            eventTypes,
            "ciphertext",
            "whsec_prefix",
            Now);
}
