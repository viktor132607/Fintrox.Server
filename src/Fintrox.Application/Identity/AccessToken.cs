namespace Fintrox.Application.Identity;

public sealed record AccessToken(
    string Value,
    DateTimeOffset ExpiresAtUtc);
