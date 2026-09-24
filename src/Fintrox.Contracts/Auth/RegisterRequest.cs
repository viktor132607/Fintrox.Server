using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record RegisterRequest(
    [property: Required, EmailAddress, MaxLength(256)] string Email,
    [property: Required, MinLength(12), MaxLength(128)] string Password,
    [property: Required, MaxLength(120)] string DisplayName);
