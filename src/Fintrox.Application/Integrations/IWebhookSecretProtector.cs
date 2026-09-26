namespace Fintrox.Application.Integrations;

public interface IWebhookSecretProtector
{
    string Protect(string plaintext);

    string Unprotect(string ciphertext);
}
