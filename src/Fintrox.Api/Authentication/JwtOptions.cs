namespace Fintrox.Api.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = null!;

    public string Audience { get; init; } = null!;

    public string SigningKey { get; init; } = null!;

    public int AccessTokenMinutes { get; init; } = 15;

    public int IntegrationAccessTokenMinutes { get; init; } = 15;
}
