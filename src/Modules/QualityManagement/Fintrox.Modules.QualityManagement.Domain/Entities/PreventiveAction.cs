namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped PreventiveAction model. References require application validation.</summary>
public sealed class PreventiveAction : OrganizationEntity
{
    public PreventiveAction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourceNonconformityId,
        string actionDescription,
        Guid ownerUserId,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        SourceNonconformityId = DomainRules.Id(sourceNonconformityId, nameof(sourceNonconformityId));
        ActionDescription = DomainRules.Text(actionDescription, nameof(actionDescription));
        OwnerUserId = DomainRules.Id(ownerUserId, nameof(ownerUserId));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid SourceNonconformityId { get; }
    public string ActionDescription { get; }
    public Guid OwnerUserId { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
