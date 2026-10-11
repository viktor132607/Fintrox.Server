namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

public sealed class FieldServiceOrder : OrganizationEntity
{
    public FieldServiceOrder(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid ticketId,
        Guid assignedResourceId,
        DateTimeOffset scheduledAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        TicketId = DomainRules.Id(ticketId, nameof(ticketId));
        AssignedResourceId = DomainRules.Id(assignedResourceId, nameof(assignedResourceId));
        ScheduledAtUtc = DomainRules.Instant(scheduledAtUtc, nameof(scheduledAtUtc));
    }

    public Guid TicketId { get; }
    public Guid AssignedResourceId { get; }
    public DateTimeOffset ScheduledAtUtc { get; }
}
