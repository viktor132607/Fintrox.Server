using Fintrox.Domain.Payments;

namespace Fintrox.Infrastructure.Persistence.Models;

public sealed class PaymentNumberSequence
{
    public Guid OrganizationId { get; set; }

    public int CalendarYear { get; set; }

    public PaymentDirection Direction { get; set; }

    public long LastNumber { get; set; }
}
