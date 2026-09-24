namespace Fintrox.Application.Identity;

public sealed class IdentityConflictException(string message) : Exception(message);
