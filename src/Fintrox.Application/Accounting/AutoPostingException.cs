namespace Fintrox.Application.Accounting;

public sealed class AutoPostingException(string message)
    : Exception(message);
