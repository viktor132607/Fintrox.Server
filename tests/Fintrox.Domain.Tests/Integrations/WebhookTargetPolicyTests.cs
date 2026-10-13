using System.Net;
using Fintrox.Domain.Integrations;
using Fintrox.Infrastructure.Integrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fintrox.Domain.Tests.Integrations;

[TestClass]
public sealed class WebhookTargetPolicyTests
{
    [TestMethod]
    public void PublicUrlsAreAcceptedAndCanonicalized()
    {
        Assert.AreEqual(
            "https://webhooks.example.com/events?source=api",
            WebhookTargetPolicy.Normalize(
                " https://WEBHOOKS.example.com/events?source=api "));
        Assert.AreEqual("http://1.1.1.1/", WebhookTargetPolicy.Normalize("http://1.1.1.1"));
        Assert.AreEqual(
            "https://[2606:4700:4700::1111]/",
            WebhookTargetPolicy.Normalize("https://[2606:4700:4700::1111]"));
    }

    [TestMethod]
    public void RegistrationAndUpdateRejectUnsafeUrls()
    {
        var blocked = new[]
        {
            "http://localhost:8080/admin",
            "http://127.0.0.1/",
            "http://127.1/",
            "http://10.0.0.12/",
            "http://172.16.0.1/",
            "http://192.168.1.1/",
            "http://169.254.169.254/latest/meta-data/",
            "http://100.100.100.200/",
            "http://100.64.0.1/",
            "http://0.0.0.0/",
            "http://224.0.0.1/",
            "http://[::1]/",
            "http://[fc00::1]/",
            "http://[fe80::1]/",
            "http://[::ffff:127.0.0.1]/",
            "http://[2001:db8::1]/",
            "http://[2002:c0a8:0101::1]/",
            "http://metadata/",
            "http://kubernetes.default.svc/",
            "http://host.local/",
            "http://host.localhost/",
            "http://host.internal/",
            "http://host.home.arpa/",
            "http://host.test/",
            "http://host.invalid/",
            "http://host.onion/",
            "http://host.corp/",
            "http://evil.com@127.0.0.1/",
            "http://admin:pass@webhooks.example.com/",
            "https://webhooks.example.com/callback#secret",
            "file:///etc/passwd",
            "ftp://webhooks.example.com/file",
            "http://0x7f000001/",
            "not a url"
        };

        foreach (var url in blocked)
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => WebhookTargetPolicy.Normalize(url), url);
        }

        var subscription = Subscription();
        Assert.ThrowsExactly<ArgumentException>(
            () => subscription.Update("Changed", "http://127.0.0.1/", "*", DateTimeOffset.UtcNow));
    }

    [TestMethod]
    public void PublicIpPredicateDeniesSpecialPurposeAddresses()
    {
        foreach (var blocked in new[]
        {
            "0.0.0.0", "10.1.2.3", "100.64.0.2", "127.0.0.1",
            "169.254.169.254", "172.30.1.2", "192.0.0.1", "192.0.2.2",
            "192.168.0.2", "198.18.0.1", "198.51.100.6", "203.0.113.1",
            "224.0.0.1", "255.255.255.255", "::1", "::", "fe80::1",
            "fc00::1", "ff02::1", "2001:db8::1", "2002::1",
            "::ffff:169.254.169.254"
        })
        {
            Assert.IsFalse(WebhookTargetPolicy.IsPublicAddress(IPAddress.Parse(blocked)), blocked);
        }

        foreach (var allowed in new[]
        {
            "1.1.1.1", "8.8.8.8", "23.42.12.1",
            "2606:4700:4700::1111", "2001:4860:4860::8888"
        })
        {
            Assert.IsTrue(WebhookTargetPolicy.IsPublicAddress(IPAddress.Parse(allowed)), allowed);
        }
    }

    [TestMethod]
    public async Task DnsResolutionRejectsPrivateAndMixedAnswers()
    {
        foreach (var answers in new[]
        {
            Array.Empty<IPAddress>(),
            new[] { IPAddress.Parse("127.0.0.1") },
            new[] { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("169.254.169.254") },
            new[] { IPAddress.Parse("2001:4860:4860::8888"), IPAddress.Parse("fc00::1") }
        })
        {
            await Assert.ThrowsExactlyAsync<WebhookTargetBlockedException>(
                async () => await WebhookOutboundTransport.ResolvePublicAsync(
                    "webhooks.example.com",
                    (_, _) => Task.FromResult(answers),
                    CancellationToken.None));
        }
    }

    [TestMethod]
    public async Task ValidPublicDnsAnswersArePinnedForSocketConnect()
    {
        var calls = 0;
        var expected = new[] { IPAddress.Parse("1.1.1.1"), IPAddress.Parse("2606:4700::1111") };
        var actual = await WebhookOutboundTransport.ResolvePublicAsync(
            "webhooks.example.com",
            (host, _) =>
            {
                calls++;
                Assert.AreEqual("webhooks.example.com", host);
                return Task.FromResult(expected);
            },
            CancellationToken.None);

        Assert.AreEqual(1, calls);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task UnsafeHostIsNeverSentToResolver()
    {
        var called = false;
        await Assert.ThrowsExactlyAsync<WebhookTargetBlockedException>(
            async () => await WebhookOutboundTransport.ResolvePublicAsync(
                "metadata.internal",
                (_, _) =>
                {
                    called = true;
                    return Task.FromResult(new[] { IPAddress.Parse("8.8.8.8") });
                },
                CancellationToken.None));

        Assert.IsFalse(called);
    }

    [TestMethod]
    public void OutboundHandlerDisablesRedirectsProxyAndCookies()
    {
        using var handler = WebhookOutboundTransport.CreateHandler();
        Assert.IsFalse(handler.AllowAutoRedirect);
        Assert.IsFalse(handler.UseProxy);
        Assert.IsFalse(handler.UseCookies);
        Assert.IsNotNull(handler.ConnectCallback);
    }

    private static WebhookSubscription Subscription() => WebhookSubscription.Create(
        Guid.NewGuid(), "Demo", "https://webhooks.example.com/receive",
        "*", "ciphertext", "prefix", DateTimeOffset.UtcNow);
}
