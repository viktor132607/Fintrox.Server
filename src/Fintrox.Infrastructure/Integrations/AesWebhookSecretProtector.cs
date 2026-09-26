using System.Security.Cryptography;
using System.Text;
using Fintrox.Application.Integrations;
using Microsoft.Extensions.Configuration;

namespace Fintrox.Infrastructure.Integrations;

public sealed class AesWebhookSecretProtector : IWebhookSecretProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesWebhookSecretProtector(IConfiguration configuration)
    {
        var keyMaterial =
            configuration["Integrations:WebhookEncryptionKey"]
            ?? configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(keyMaterial) ||
            keyMaterial.Length < 32)
        {
            throw new InvalidOperationException(
                "Integrations:WebhookEncryptionKey or Jwt:SigningKey must contain at least 32 characters.");
        }

        _key = SHA256.HashData(
            Encoding.UTF8.GetBytes(keyMaterial));
    }

    public string Protect(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(
            nonce,
            plaintextBytes,
            ciphertext,
            tag);

        var payload = new byte[
            NonceSize +
            TagSize +
            ciphertext.Length];

        Buffer.BlockCopy(
            nonce,
            0,
            payload,
            0,
            nonce.Length);
        Buffer.BlockCopy(
            tag,
            0,
            payload,
            NonceSize,
            tag.Length);
        Buffer.BlockCopy(
            ciphertext,
            0,
            payload,
            NonceSize + TagSize,
            ciphertext.Length);

        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);

        byte[] payload;

        try
        {
            payload = Convert.FromBase64String(ciphertext);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Webhook signing secret ciphertext is invalid.",
                exception);
        }

        if (payload.Length <= NonceSize + TagSize)
        {
            throw new InvalidOperationException(
                "Webhook signing secret ciphertext is invalid.");
        }

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var encrypted = payload.AsSpan(NonceSize + TagSize);
        var plaintext = new byte[encrypted.Length];

        try
        {
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(
                nonce,
                encrypted,
                tag,
                plaintext);
        }
        catch (AuthenticationTagMismatchException exception)
        {
            throw new InvalidOperationException(
                "Webhook signing secret could not be decrypted.",
                exception);
        }

        return Encoding.UTF8.GetString(plaintext);
    }
}
