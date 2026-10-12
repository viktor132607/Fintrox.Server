namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped SupportTicketAssignment domain record. No cross-module navigation properties.</summary>
public sealed class SupportTicketAssignment : OrganizationEntity
{
    public SupportTicketAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid assignedUserId,
        DateTimeOffset assignedAtUtc,
        string assignmentReason)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        AssignedUserId = DomainRules.Id(assignedUserId, nameof(assignedUserId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
        AssignmentReason = DomainRules.Text(assignmentReason, nameof(assignmentReason));
    }

    public Guid SupportTicketId { get; }
    public Guid AssignedUserId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
    public string AssignmentReason { get; }
}
