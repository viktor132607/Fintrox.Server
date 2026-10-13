using System.Net;
using System.Net.Sockets;

namespace Fintrox.Domain.Integrations;

/// <summary>
/// Structural webhook target validation. DNS addresses are checked again at socket-connect time.
/// Neither validation depends on the configured DNS server being trustworthy.
/// </summary>
public static class WebhookTargetPolicy
{
    public static string Normalize(string? targetUrl)
    {
        if (string.IsNullOrWhiteSpace(targetUrl) ||
            targetUrl.Trim().Length > 2048 ||
            !Uri.TryCreate(targetUrl.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !IsAllowedHost(uri.IdnHost))
        {
            throw new ArgumentException(
                "Webhook target must be a public HTTP(S) URL without credentials or fragments.",
                nameof(targetUrl));
        }

        return uri.AbsoluteUri;
    }

    public static bool IsAllowedHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        var normalized = host.TrimEnd('.');
        if (IPAddress.TryParse(normalized, out var address))
        {
            return IsPublicAddress(address);
        }

        // Only fully qualified public DNS hostnames. Do not accept search-domain
        // aliases, local namespaces, or alternate numeric forms of IP literals.
        if (!normalized.Contains('.', StringComparison.Ordinal) ||
            normalized.StartsWith('.', StringComparison.Ordinal) ||
            normalized.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".internal", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".lan", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".test", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".onion", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".arpa", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".home", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".corp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var labels = normalized.Split('.');
        if (labels.Any(label =>
                label.Length is < 1 or > 63 ||
                label[0] == '-' ||
                label[^1] == '-' ||
                !label.All(c => char.IsAsciiLetterOrDigit(c) || c == '-')))
        {
            return false;
        }

        // Reject hostnames disguised as decimal/hex/dotted-octal IP literals;
        // the socket resolver still enforces public-only results.
        return !normalized.All(c =>
            char.IsAsciiDigit(c) || c is '.' or 'x' or 'X' ||
            c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F');
    }

    public static bool IsPublicAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (address.IsIPv4MappedToIPv6)
        {
            return IsPublicAddress(address.MapToIPv4());
        }

        var octets = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var a = octets[0];
            var b = octets[1];
            var c = octets[2];

            return a is > 0 and < 224 &&
                   a != 10 && a != 127 &&
                   !(a == 100 && b is >= 64 and <= 127) &&
                   !(a == 169 && b == 254) &&
                   !(a == 172 && b is >= 16 and <= 31) &&
                   !(a == 192 && (b == 0 && c is 0 or 2 ||
                                  b == 88 && c == 99 ||
                                  b == 168)) &&
                   !(a == 198 && (b is 18 or 19 ||
                                  b == 51 && c == 100)) &&
                   !(a == 203 && b == 0 && c == 113);
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6 ||
            address.IsIPv6LinkLocal ||
            address.IsIPv6Multicast ||
            address.IsIPv6SiteLocal ||
            IPAddress.IsLoopback(address))
        {
            return false;
        }

        // Only global unicast 2000::/3, excluding IETF special-purpose 2001::/23,
        // documentation 2001:db8::/32 and 6to4 (which embeds an IPv4 destination).
        return (octets[0] & 0xe0) == 0x20 &&
               !(octets[0] == 0x20 && octets[1] == 0x01 &&
                 octets[2] is 0x00 or 0x01) &&
               !(octets[0] == 0x20 && octets[1] == 0x01 &&
                 octets[2] == 0x0d && octets[3] == 0xb8) &&
               !(octets[0] == 0x20 && octets[1] == 0x02);
    }

    public static IPAddress[] RequirePublicDnsAnswers(
        string host,
        IPAddress[] addresses)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(addresses);

        if (!IsAllowedHost(host) || addresses.Length == 0 ||
            addresses.Any(address => !IsPublicAddress(address)))
        {
            throw new WebhookTargetBlockedException(
                "Webhook destination is not a publicly routable host.");
        }

        return addresses;
    }
}

public sealed class WebhookTargetBlockedException(string message)
    : HttpRequestException(message);
