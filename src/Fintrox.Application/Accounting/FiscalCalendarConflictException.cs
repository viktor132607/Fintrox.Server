namespace Fintrox.Application.Accounting;

public sealed class FiscalCalendarConflictException(string message)
    : Exception(message);
