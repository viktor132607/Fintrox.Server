namespace Fintrox.Application.Identity;

public sealed class AuthenticationException(string message) : Exception(message);
