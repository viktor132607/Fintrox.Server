using Fintrox.Domain.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Integrations;

[TestClass]
public sealed class IntegrationRequestRecordTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid IntegrationClientId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateTimeOffset Now =
        new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    private const string Hash =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [TestMethod]
    public void CreateNormalizesExternalEventKey()
    {
        var record = Create();

        Assert.AreEqual("shop-api", record.SourceSystem);
        Assert.AreEqual("Order-42", record.ExternalId);
        Assert.AreEqual("order.created", record.EventType);
        Assert.AreEqual("POST", record.RequestMethod);
        Assert.AreEqual(
            IntegrationRequestStatus.Processing,
            record.Status);
    }

    [TestMethod]
    public void CompleteStoresReplayDataAndResourceReference()
    {
        var record = Create();

        record.Complete(
            201,
            "application/json",
            "{\"id\":\"42\"}",
            "/api/v1/sales/invoices/42",
            Now.AddSeconds(1));

        Assert.AreEqual(
            IntegrationRequestStatus.Completed,
            record.Status);
        Assert.AreEqual(201, record.ResponseStatusCode);
        Assert.AreEqual(
            "/api/v1/sales/invoices/42",
            record.ResourceReference);
        Assert.AreEqual(
            Now.AddSeconds(1),
            record.CompletedAtUtc);
    }

    [TestMethod]
    public void CompletedRecordCannotBeCompletedTwice()
    {
        var record = Create();

        record.Complete(
            200,
            "application/json",
            "{}",
            "/resource",
            Now.AddSeconds(1));

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            record.Complete(
                200,
                "application/json",
                "{}",
                "/resource",
                Now.AddSeconds(2)));
    }

    [TestMethod]
    public void InvalidHashIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            IntegrationRequestRecord.Create(
                OrganizationId,
                IntegrationClientId,
                "source",
                "external",
                "event",
                "POST",
                "/api/test",
                "not-a-sha256-hash",
                Now));
    }

    private static IntegrationRequestRecord Create() =>
        IntegrationRequestRecord.Create(
            OrganizationId,
            IntegrationClientId,
            " Shop-API ",
            " Order-42 ",
            " Order.Created ",
            "post",
            "/api/v1/sales/invoices",
            Hash,
            Now);
}
