namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityContainmentAction model. References require application validation.</summary>
public sealed class QualityContainmentAction : OrganizationEntity
{
    public QualityContainmentAction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid nonconformityId,
        string actionDescription,
        Guid ownerUserId,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        NonconformityId = DomainRules.Id(nonconformityId, nameof(nonconformityId));
        ActionDescription = DomainRules.Text(actionDescription, nameof(actionDescription));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid NonconformityId { get; }
    public string ActionDescription { get; }
    public Guid OwnerUserId { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
