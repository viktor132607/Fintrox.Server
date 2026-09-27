namespace Fintrox.Infrastructure.Persistence.Models;

public sealed class JournalNumberSequence
{
    public Guid OrganizationId { get; set; }

    public Guid FiscalYearId { get; set; }

    public long LastNumber { get; set; }
}
