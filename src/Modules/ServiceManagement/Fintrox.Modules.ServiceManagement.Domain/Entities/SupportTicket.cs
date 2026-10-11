namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

public sealed class SupportTicket : OrganizationEntity
{
    public SupportTicket(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string subject,
        string description,
        TicketPriority priority)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Subject = DomainRules.Text(subject, nameof(subject));
        Description = DomainRules.Text(description, nameof(description));
        Priority = DomainRules.Defined(priority, nameof(priority));
    }

    public Guid CounterpartyId { get; }
    public string Subject { get; }
    public string Description { get; }
    public TicketPriority Priority { get; }
}
