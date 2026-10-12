namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityEffectivenessCheck model. References require application validation.</summary>
public sealed class QualityEffectivenessCheck : OrganizationEntity
{
    public QualityEffectivenessCheck(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid correctiveActionId,
        DateOnly checkedOn,
        Guid checkedByUserId,
        string outcome)
        : base(id, organizationId, createdAtUtc)
    {
        CorrectiveActionId = DomainRules.Id(correctiveActionId, nameof(correctiveActionId));
        CheckedOn = DomainRules.Date(checkedOn, nameof(checkedOn));
        CheckedByUserId = DomainRules.Id(checkedByUserId, nameof(checkedByUserId));
        Outcome = DomainRules.Text(outcome, nameof(outcome));
    }

    public Guid CorrectiveActionId { get; }
    public DateOnly CheckedOn { get; }
    public Guid CheckedByUserId { get; }
    public string Outcome { get; }
}
