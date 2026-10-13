using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record RegisterRequest(
    [param: Required, EmailAddress, MaxLength(256)] string Email,
    [param: Required, MinLength(12), MaxLength(128)] string Password,
    [param: Required, MaxLength(120)] string DisplayName);
