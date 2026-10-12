namespace Fintrox.Modules.CRM.Domain.Entities;

/// <summary>Tenant-owned CrmTask record. Domain invariants enforced at construction.</summary>
public sealed class CrmTask : OrganizationEntity
{
    public CrmTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid ownerUserId,
        Guid relatedRecordId,
        string subject,
        DateOnly dueOn,
        string priority,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        RelatedRecordId = DomainRules.Id(relatedRecordId, nameof(relatedRecordId));
        Subject = DomainRules.Text(subject, nameof(subject));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Priority = DomainRules.Text(priority, nameof(priority));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid OwnerUserId { get; }
    public Guid RelatedRecordId { get; }
    public string Subject { get; }
    public DateOnly DueOn { get; }
    public string Priority { get; }
    public string Status { get; }
}
