namespace Fintrox.Infrastructure.Persistence.Models;

public sealed class SalesInvoiceNumberSequence
{
    public Guid OrganizationId { get; set; }

    public int CalendarYear { get; set; }

    public long LastNumber { get; set; }
}
