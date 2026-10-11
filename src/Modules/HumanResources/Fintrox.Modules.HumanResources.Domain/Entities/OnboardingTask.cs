namespace Fintrox.Modules.HumanResources.Domain.Entities;

public sealed class OnboardingTask : OrganizationEntity
{
    public OnboardingTask(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string description,
        DateOnly dueOn)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        Description = DomainRules.Text(description, nameof(description));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
    }

    public Guid EmployeeId { get; }
    public string Description { get; }
    public DateOnly DueOn { get; }
}
