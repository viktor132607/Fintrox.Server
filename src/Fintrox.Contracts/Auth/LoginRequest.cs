using System.ComponentModel.DataAnnotations;

namespace Fintrox.Contracts.Auth;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(256)] string Email,
    [param: Required, MaxLength(128)] string Password);
