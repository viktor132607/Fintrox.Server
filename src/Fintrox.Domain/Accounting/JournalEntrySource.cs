namespace Fintrox.Domain.Accounting;

public enum JournalEntrySource
{
    Manual = 0,
    Integration = 10,
    Import = 20,
    System = 30
}
