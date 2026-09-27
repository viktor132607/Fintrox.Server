namespace Fintrox.Application.Accounting;

public sealed class AutoPostingRuleConflictException(string message)
    : Exception(message);
