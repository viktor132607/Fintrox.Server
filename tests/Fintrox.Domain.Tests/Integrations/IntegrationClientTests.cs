using Fintrox.Domain.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Integrations;

[TestClass]
public sealed class IntegrationClientTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly DateTimeOffset Now =
        new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void CreateStoresCredentialMetadataAndStartsActive()
    {
        var client = CreateClient();

        Assert.AreEqual("fic_test", client.ClientId);
        Assert.AreEqual("Demo connector", client.Name);
        Assert.AreEqual("hash", client.SecretHash);
        Assert.AreEqual("fis_prefix", client.SecretPrefix);
        Assert.AreEqual(
            "sales.read sales.write",
            client.Scopes);
        Assert.IsTrue(client.IsActive);
        Assert.AreEqual(Now, client.SecretRotatedAtUtc);
        Assert.IsNull(client.LastUsedAtUtc);
    }

    [TestMethod]
    public void RotateSecretReplacesHashAndPrefix()
    {
        var client = CreateClient();

        client.RotateSecret(
            "next-hash",
            "fis_next",
            Now.AddMinutes(1));

        Assert.AreEqual("next-hash", client.SecretHash);
        Assert.AreEqual("fis_next", client.SecretPrefix);
        Assert.AreEqual(
            Now.AddMinutes(1),
            client.SecretRotatedAtUtc);
    }

    [TestMethod]
    public void UpdateChangesNameAndScopes()
    {
        var client = CreateClient();

        client.Update(
            "Updated connector",
            "reports.read",
            Now.AddMinutes(1));

        Assert.AreEqual(
            "Updated connector",
            client.Name);
        Assert.AreEqual(
            "reports.read",
            client.Scopes);
    }

    [TestMethod]
    public void MarkUsedCapturesUsageTimestamp()
    {
        var client = CreateClient();

        client.MarkUsed(Now.AddMinutes(2));

        Assert.AreEqual(
            Now.AddMinutes(2),
            client.LastUsedAtUtc);
    }

    [TestMethod]
    public void ClientCanBeDeactivatedAndReactivated()
    {
        var client = CreateClient();

        client.Deactivate(Now.AddMinutes(1));
        Assert.IsFalse(client.IsActive);

        client.Activate(Now.AddMinutes(2));
        Assert.IsTrue(client.IsActive);
    }

    private static IntegrationClient CreateClient() =>
        IntegrationClient.Create(
            OrganizationId,
            "fic_test",
            "Demo connector",
            "hash",
            "fis_prefix",
            "sales.read sales.write",
            Now);
}
