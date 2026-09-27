using System.Security.Cryptography;

namespace Fintrox.Application.Integrations;

public static class WebhookSecretUtility
{
    public static string Generate() =>
        $"whsec_{Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant()}";

    public static string Prefix(string secret) =>
        secret.Length <= 14
            ? secret
            : secret[..14];
}
