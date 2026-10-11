namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class CorrectiveAction : OrganizationEntity
{
    public CorrectiveAction(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid nonconformityId,
        string description,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        NonconformityId = DomainRules.Id(nonconformityId, nameof(nonconformityId));
        Description = DomainRules.Text(description, nameof(description));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
    }

    public Guid NonconformityId { get; }
    public string Description { get; }
    public DateOnly DueOn { get; }
}
