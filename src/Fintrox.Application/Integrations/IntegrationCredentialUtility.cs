using System.Security.Cryptography;
using System.Text;

namespace Fintrox.Application.Integrations;

public static class IntegrationCredentialUtility
{
    public static string GenerateClientId() =>
        $"fic_{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()}";

    public static string GenerateSecret() =>
        $"fis_{Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant()}";

    public static string HashSecret(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(secret)))
            .ToLowerInvariant();
    }

    public static bool VerifySecret(
        string secret,
        string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        byte[] expected;

        try
        {
            expected = Convert.FromHexString(expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = SHA256.HashData(
            Encoding.UTF8.GetBytes(secret));

        return CryptographicOperations.FixedTimeEquals(
            actual,
            expected);
    }

    public static string GetSecretPrefix(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        return secret.Length <= 12
            ? secret
            : secret[..12];
    }
}
