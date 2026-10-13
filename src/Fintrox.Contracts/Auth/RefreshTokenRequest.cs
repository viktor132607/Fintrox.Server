using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record RefreshTokenRequest(
    [param: Required, MaxLength(1024)] string RefreshToken);
