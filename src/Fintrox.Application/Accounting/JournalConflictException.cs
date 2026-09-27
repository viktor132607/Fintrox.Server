namespace Fintrox.Application.Accounting;

public sealed class JournalConflictException(string message) : Exception(message);
