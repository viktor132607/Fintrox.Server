namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped PeriodCloseTask domain record; referenced objects are validated by application use cases.</summary>
public sealed class PeriodCloseTask : OrganizationEntity
{
    public PeriodCloseTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid checklistId,
        string code,
        string description,
        string assignedRole,
        bool isMandatory)
        : base(id, organizationId, createdAtUtc)
    {
        ChecklistId = DomainRules.Id(checklistId, nameof(checklistId));
        Code = DomainRules.Text(code, nameof(code));
        Description = DomainRules.Text(description, nameof(description));
        AssignedRole = DomainRules.Text(assignedRole, nameof(assignedRole));
        IsMandatory = isMandatory;
    }

    public Guid ChecklistId { get; }
    public string Code { get; }
    public string Description { get; }
    public string AssignedRole { get; }
    public bool IsMandatory { get; }
}
