namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned LeadAssignment record. Domain invariants enforced at construction.</summary>
public sealed class LeadAssignment : OrganizationEntity
{
    public LeadAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid leadId,
        Guid ownerUserId,
        DateTimeOffset assignedAtUtc,
        string assignmentReason)
        : base(id, organizationId, createdAtUtc)
    {
        LeadId = DomainRules.Id(leadId, nameof(leadId));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
        AssignmentReason = DomainRules.Text(assignmentReason, nameof(assignmentReason));
    }

    public Guid LeadId { get; }
    public Guid OwnerUserId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
    public string AssignmentReason { get; }
}
