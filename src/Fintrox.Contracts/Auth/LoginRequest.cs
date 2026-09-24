using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record LoginRequest(
    [property: Required, EmailAddress, MaxLength(256)] string Email,
    [property: Required, MaxLength(128)] string Password);
