using System.Net;
using System.Net.Sockets;
using Fintrox.Domain.Integrations;

namespace Fintrox.Infrastructure.Integrations;

/// <summary>
/// Outbound-only webhook transport: resolve and validate the DNS answer used by
/// each new socket, connect to that exact IP (no second DNS lookup), disable
/// HTTP redirects and proxies, and keep TLS name validation on the URL hostname.
/// </summary>
public static class WebhookOutboundTransport
{
    public static SocketsHttpHandler CreateHandler() => new()
    {
        AllowAutoRedirect = false,
        UseProxy = false,
        UseCookies = false,
        ConnectTimeout = TimeSpan.FromSeconds(10),
        ConnectCallback = ConnectPublicAsync
    };

    public static async ValueTask<IPAddress[]> ResolvePublicAsync(
        string host,
        Func<string, CancellationToken, Task<IPAddress[]>> resolver,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        if (!WebhookTargetPolicy.IsAllowedHost(host))
        {
            throw new WebhookTargetBlockedException(
                "Webhook destination hostname is blocked.");
        }

        // The result is validated as a whole. Mixed public/private DNS answers
        // are rejected, not silently filtered or retried via another resolver.
        var addresses = await resolver(host, cancellationToken);
        return WebhookTargetPolicy.RequirePublicDnsAnswers(host, addresses);
    }

    private static async ValueTask<Stream> ConnectPublicAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        var addresses = await ResolvePublicAsync(
            context.DnsEndPoint.Host,
            static (host, token) => Dns.GetHostAddressesAsync(host, token),
            cancellationToken);

        // Public addresses come from one validated resolution. Never pass a
        // hostname to Socket.ConnectAsync, which would perform a second lookup.
        // Try only IPs from that validated answer set.
        Exception? lastError = null;
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(
                    new IPEndPoint(address, context.DnsEndPoint.Port),
                    cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex) when (ex is SocketException or IOException)
            {
                lastError = ex;
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        throw new HttpRequestException(
            "Webhook destination could not be reached.", lastError);
    }
}
