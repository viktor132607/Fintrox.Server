using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record RevokeRefreshTokenRequest(
    [property: Required, MaxLength(1024)] string RefreshToken);
