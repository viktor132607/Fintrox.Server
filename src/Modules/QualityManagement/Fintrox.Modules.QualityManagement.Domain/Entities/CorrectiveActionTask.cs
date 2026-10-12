namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped CorrectiveActionTask model. References require application validation.</summary>
public sealed class CorrectiveActionTask : OrganizationEntity
{
    public CorrectiveActionTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid correctiveActionId,
        Guid assignedUserId,
        string description,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CorrectiveActionId = DomainRules.Id(correctiveActionId, nameof(correctiveActionId));
        AssignedUserId = DomainRules.Id(assignedUserId, nameof(assignedUserId));
        Description = DomainRules.Text(description, nameof(description));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CorrectiveActionId { get; }
    public Guid AssignedUserId { get; }
    public string Description { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
