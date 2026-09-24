namespace Fintrox.Application.Accounting;

public sealed class AccountConflictException(string message) : Exception(message);
